namespace Jobsity.StockChallenge.Application.Chat
{
    public interface IChatNotificationService
    {
        Task NotifyMessageAsync(ChatMessageDto message, CancellationToken cancellationToken = default);
    }
}
