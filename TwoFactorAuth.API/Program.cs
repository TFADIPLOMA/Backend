using BOCRM.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System;
using System.Text;
using TwoFactorAuth.API.Contextes;
using TwoFactorAuth.API.Dtos;
using TwoFactorAuth.API.Middlewares;
using TwoFactorAuth.API.Repository;
using TwoFactorAuth.API.Services;
using TwoFactorAuth.API.SocketHubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddSingleton<TokenService>(sp =>
{
    return new TokenService(jwtSettings.Issuer, jwtSettings.Audience, jwtSettings.SecretKey);
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://tfadiploma.ru",         
                "null" //для андройд-приложения 
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("X-Guid");
    });
});

builder.Services.AddScoped<UserRepository>();

builder.Services.AddScoped<IPasswordHasher<string>,PasswordHasher<string>>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FaceRecognitionService>();
builder.Services.AddScoped<QRCoderService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddMemoryCache();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TFA")
        .WriteTo.Console()
        .WriteTo.PostgreSQL(
            connectionString: context.Configuration.GetConnectionString("PostgresConnection"),
            tableName: "logs",
            needAutoCreateTable: true,
            columnOptions: new Dictionary<string, ColumnWriterBase>
            {
                ["message"] = new RenderedMessageColumnWriter(),
                ["message_template"] = new MessageTemplateColumnWriter(),
                ["level"] = new LevelColumnWriter(),
                ["timestamp"] = new TimestampColumnWriter(),
                ["exception"] = new ExceptionColumnWriter(),
                ["log_event"] = new LogEventSerializedColumnWriter(),
                ["trace_id"] = new SinglePropertyColumnWriter("traceId"),
                ["user_name"] = new SinglePropertyColumnWriter("userName")
            });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapHub<AuthHub>("/authHub");
app.MapHub<QrLoginHub>("/qrlink").RequireCors("AllowAll");

app.UseSerilogRequestLogging();

app.Run();
