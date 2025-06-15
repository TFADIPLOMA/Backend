using Microsoft.AspNetCore.Identity;
using TwoFactorAuth.API.Dtos;
using TwoFactorAuth.API.Models;
using TwoFactorAuth.API.Repository;
using TwoFactorAuth.API.Services;

namespace BOCRM.Application.Services
{
    public class AuthService(
        UserService userService,
        IPasswordHasher<string> passwordHasher,
        TokenService tokenService
        )
    {
        private readonly UserService _userService = userService;
        private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;
        private readonly TokenService _tokenService = tokenService;

        public async Task<LoginResponseDto> Login(LoginRequestDto request)
        {
            var user = await _userService.GetUserByEmail(request.Email) ?? throw new Exception("USER_NOT_FOUND");

            var passwordValidationResult = _passwordHasher.VerifyHashedPassword(request.Email, user.Password, request.Password);
            if (passwordValidationResult == PasswordVerificationResult.Failed)
            {
                throw new Exception("INCORRECT_PASSWORD");
            }

            return new()
            {
                User = new UserDto(user),
                AccessToken = _tokenService.GenerateToken(user, TimeSpan.FromMinutes(25)),
                RefreshToken = _tokenService.GenerateToken(user, TimeSpan.FromHours(10))
            };
        }

        public async Task<User> Registration(RegistrationRequestDto request)
        {
            var user = await _userService.CreateUser(new User()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = _passwordHasher.HashPassword(request.Email, request.Password),
            });
            return user;
        }

        public async Task<LoginResponseDto> Refresh(string email)
        {
            var user = await _userService.GetUserByEmail(email) ?? throw new Exception("USER_NOT_FOUND");
            return new()
            {
                User = new UserDto(user),
                AccessToken = _tokenService.GenerateToken(user, TimeSpan.FromMinutes(25)),
                RefreshToken = _tokenService.GenerateToken(user, TimeSpan.FromHours(10))
            };
        }
    }
}
