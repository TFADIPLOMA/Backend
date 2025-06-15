using TwoFactorAuth.API.Contextes;
using TwoFactorAuth.API.Models;
using TwoFactorAuth.API.Repository;

namespace TwoFactorAuth.API.Services
{
    public class UserService(UserRepository userRepository)
    {
        private readonly UserRepository _userRepository = userRepository;

        public async Task<User?> GetUserById(Guid id) => await _userRepository.GetByIdAsync(id);
        public async Task<User?> GetUserByEmail(string email) => await _userRepository.GetByEmailAsync(email);
        public async Task<User> CreateUser(User user)
        {
            var existUser = await this.GetUserByEmail(user.Email);
            if(existUser!=null)
            {
                throw new Exception("USER_ALREADY_EXIST");
            }
            await _userRepository.AddAsync(user);
            return user;
        }
        public async Task<User> UpdateUser(User user)=>await _userRepository.UpdateAsync(user);

    }
}
