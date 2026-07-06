namespace Jobsity.StockChallenge.Application.Chat.Queries
{
    public interface IGetRecentChatMessagesHandler
    {
        Task<IReadOnlyList<ChatMessageDto>> HandleAsync(GetRecentChatMessagesQuery query, CancellationToken cancellationToken = default);
    }
}
