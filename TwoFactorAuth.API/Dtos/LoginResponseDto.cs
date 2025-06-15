namespace TwoFactorAuth.API.Dtos
{
    public class LoginResponseDto
    {
        public required UserDto User { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
