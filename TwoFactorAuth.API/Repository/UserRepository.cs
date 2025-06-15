using Microsoft.EntityFrameworkCore;
using TwoFactorAuth.API.Contextes;
using TwoFactorAuth.API.Models;

namespace TwoFactorAuth.API.Repository
{
    public class UserRepository(AppDbContext context)
    {
        private readonly AppDbContext _context = context;
        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            return entity;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var entity = await _context.Users
                .FirstOrDefaultAsync(m => m.Email == email);

            return entity;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
