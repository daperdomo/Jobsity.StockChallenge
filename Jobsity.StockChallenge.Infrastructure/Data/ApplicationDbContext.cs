using Jobsity.StockChallenge.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jobsity.StockChallenge.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(message => message.Id);
                entity.Property(message => message.SenderId).HasMaxLength(450).IsRequired();
                entity.Property(message => message.SenderUserName).HasMaxLength(256).IsRequired();
                entity.Property(message => message.Message).HasMaxLength(1000).IsRequired();
                entity.Property(message => message.ChatRoom).HasMaxLength(100).IsRequired();
                entity.Property(message => message.Timestamp).IsRequired();
                entity.HasIndex(message => new { message.ChatRoom, message.Timestamp });
            });
        }
    }
}
