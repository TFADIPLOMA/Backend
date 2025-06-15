using Newtonsoft.Json;
using System.Net;

namespace TwoFactorAuth.API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500

            if (exception.Message.Contains("NOT_FOUND"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            } 
            else if(exception.Message.Contains("ALREADY_EXIST"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
            else if (exception.Message.Contains("INCORRECT"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            var result = JsonConvert.SerializeObject(new { error = exception.Message, data = exception.Data });
            return context.Response.WriteAsync(result);
        }
    }
}
