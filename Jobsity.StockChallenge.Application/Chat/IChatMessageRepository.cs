using Jobsity.StockChallenge.Domain.Entities;

namespace Jobsity.StockChallenge.Application.Chat
{
    public interface IChatMessageRepository
    {
        Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChatMessage>> GetRecentAsync(string chatRoom, int count, CancellationToken cancellationToken = default);
    }
}
