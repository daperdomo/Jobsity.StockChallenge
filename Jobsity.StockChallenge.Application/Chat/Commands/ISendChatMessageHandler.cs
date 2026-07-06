namespace Jobsity.StockChallenge.Application.Chat.Commands
{
    public interface ISendChatMessageHandler
    {
        Task<SendChatMessageResult> HandleAsync(SendChatMessageCommand command, CancellationToken cancellationToken = default);
    }
}
