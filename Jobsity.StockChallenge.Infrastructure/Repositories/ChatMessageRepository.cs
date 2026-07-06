using Jobsity.StockChallenge.Application.Chat;
using Jobsity.StockChallenge.Domain.Entities;
using Jobsity.StockChallenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobsity.StockChallenge.Infrastructure.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatMessageRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
        {
            _dbContext.ChatMessages.Add(message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ChatMessage>> GetRecentAsync(string chatRoom, int count, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatMessages
                .Where(message => message.ChatRoom == chatRoom)
                .OrderByDescending(message => message.Timestamp)
                .Take(count)
                .OrderBy(message => message.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}
