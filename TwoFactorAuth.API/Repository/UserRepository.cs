using Microsoft.EntityFrameworkCore;
using TwoFactorAuth.API.Contextes;
using TwoFactorAuth.API.Dtos;
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

        public async Task SaveFCMToken(string email, string token)
        {
            var user = await GetByEmailAsync(email);
            if (user != null)
            {
                var existToken = await _context.FCMTokens.FirstOrDefaultAsync(m => m.UserId == user.Id && m.Token == token);
                if(existToken!=null)
                {
                    return;
                }
                await _context.FCMTokens.AddAsync(new FCMToken()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = token
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetFCMTokenByEmail(string email)
        {
            var user = await GetByEmailAsync(email);
            if(user !=null)
            {
                var fcmTokens = await _context.FCMTokens.Where(m => m.UserId == user.Id).ToListAsync();
                return fcmTokens.Select(m => m.Token).ToList();
            }
            return [];           
        }
    }
}
