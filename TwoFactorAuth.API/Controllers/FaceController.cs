using BOCRM.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TwoFactorAuth.API.Dtos;
using TwoFactorAuth.API.Services;

namespace TwoFactorAuth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private readonly FaceRecognitionService _faceService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FaceController> _logger;
        private readonly AuthService _authService;

        public FaceController(FaceRecognitionService faceService, IWebHostEnvironment env, ILogger<FaceController> logger, AuthService authService)
        {
            _faceService = faceService;
            _env = env;
            _logger = logger;
            _authService = authService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(SaveUserFaceRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Файл не был загружен.");

            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            await _faceService.Registration(request.Email, fileBytes);
            var data = await _authService.Refresh(request.Email);
            return Ok(data);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromForm] SaveUserFaceRequest request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                    return Conflict("Файл не был загружен.");

                using var memoryStream = new MemoryStream();
                await request.File.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                // Сохраняем файл в wwwroot/uploads
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                var result = await _faceService.CheckFace(request.Email, fileBytes);
                return result ? Ok() : Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при проверки лица");
                return NotFound();
            }
        }
    }
}
