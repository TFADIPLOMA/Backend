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

        public FaceController(FaceRecognitionService faceService)
        {
            _faceService = faceService;
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
            return Ok();
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(SaveUserFaceRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Файл не был загружен.");

            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            var result = await _faceService.CheckFace(request.Email, fileBytes);
            return result ? Ok() : Unauthorized();
        }
    }
}
