using BOCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TwoFactorAuth.API.Dtos;
using TwoFactorAuth.API.Models;
using TwoFactorAuth.API.Services;
using TwoFactorAuth.API.SocketHubs;

namespace TwoFactorAuth.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController(
        AuthService authService,
        IMemoryCache cach,
        IConfiguration config,
        MailService mailService,
        QRCoderService qRCoderService,
        IHubContext<AuthHub> hubContext,
        IHubContext<QrLoginHub> QRhubContext,
        UserService userService,
        FcmService fcmService
        ) : ControllerBase
    {
        private readonly AuthService _authService = authService;
        private readonly IMemoryCache _cache = cach;
        private readonly IConfiguration _config = config;
        private readonly MailService _mailService = mailService;
        private readonly QRCoderService _qRCoderService = qRCoderService;
        private readonly IHubContext<AuthHub> _hubContext = hubContext;
        private readonly IHubContext<QrLoginHub> _QRhubContext = QRhubContext;
        private readonly UserService _userService = userService;
        private readonly FcmService _fcmService = fcmService;

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var data = await _authService.Login(request);
            await SendEmailCode(new EmailRequest()
            {
                Email = request.Email
            });
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationRequestDto requestDto)
        {
            var user = await _authService.Registration(requestDto);
            return Created("/", new UserDto(user));
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Refresh()
        {
            var data = await _authService.Refresh(User.Identity.Name);
            return Ok(data);
        }

        private async Task SendEmailCode(EmailRequest request)
        {
            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set(request.Email, otp, TimeSpan.FromMinutes(5));

            _mailService.SendEmail(request.Email, otp);

            var tokens = await _userService.GetFCMTokenByEmail(request.Email);
            foreach (var token in tokens)
            {
                await _fcmService.SendNotificationAsync(token, "Подтверждение входа", $"Код для подтверждения входа: {otp}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyRequest request)
        {
            if (!_cache.TryGetValue(request.Email, out string? storedOtp) || storedOtp != request.Code)
                return BadRequest("Invalid or expired code");

            _cache.Remove(request.Email);

            var data = await _authService.Refresh(request.Email);
            return Ok(data);
        }

        [HttpGet]
        public IActionResult GetQrCode()
        {
            var guid = Guid.NewGuid();
            _cache.Set(guid.ToString(), guid.ToString(), TimeSpan.FromMinutes(5));
            var fileBytes = _qRCoderService.Generate(guid.ToString());
            Response.Headers.Add("X-Guid", guid.ToString());
            return File(fileBytes, "image/png", $"qrcode.png");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyQrCode([FromBody] VerifyRequest request)
        {
            if (!_cache.TryGetValue(request.Code, out string? value) || value != request.Code)
                return BadRequest("Invalid or expired code");

            var data = await _authService.Refresh(request.Email);


            await _QRhubContext.Clients.Group(request.Code).SendAsync("QrLoginSuccess", data);

            _cache.Remove(request.Code);

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserByEmail(GetUserByEmailRequest request)
        {
            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserFCMToken(VerifyRequest request)
        {
            await _userService.SaveFCMToken(request.Email, request.Code);
            return NoContent();
        }
    }
}
