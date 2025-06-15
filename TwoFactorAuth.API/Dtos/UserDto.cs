using TwoFactorAuth.API.Models;

namespace TwoFactorAuth.API.Dtos
{
    public class UserDto(User user)
    {
        public Guid Id { get; set; } = user.Id;
        public string Email { get; set; } = user.Email;
        public string FirstName { get; set; } = user.FirstName;
        public string LastName { get; set; } = user.LastName;
    }
}
