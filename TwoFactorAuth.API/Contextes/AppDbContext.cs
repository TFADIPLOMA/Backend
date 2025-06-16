using Microsoft.EntityFrameworkCore;
using TwoFactorAuth.API.Models;

namespace TwoFactorAuth.API.Contextes
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FaceEmbedding> FaceEmbeddings { get; set; }
        public DbSet<FCMToken> FCMTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
