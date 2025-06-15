using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TwoFactorAuth.API.Models;

namespace TwoFactorAuth.API.Services
{
    public class TokenService(string issuer, string audience, string key)
    {
        private readonly string _issuer = issuer;
        private readonly string _audience = audience;
        private readonly string _key = key;

        public string GenerateToken(User user, TimeSpan expires)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.Add(expires),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (bool isValid, Guid userId) ValidateRefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateLifetime = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero // Optional: reduce the default clock skew if necessary
            };

            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);

                // Extract user ID from the claims
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return (true, userId != null ? new Guid(userId) : Guid.Empty);
            }
            catch (SecurityTokenException)
            {
                // Token validation failed
                return (false, Guid.Empty);
            }
        }
    }
}
