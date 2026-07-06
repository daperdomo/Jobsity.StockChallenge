namespace Jobsity.StockChallenge.Application.Chat.Commands
{
    public interface ISaveBotMessageHandler
    {
        Task<ChatMessageDto> HandleAsync(SaveBotMessageCommand command, CancellationToken cancellationToken = default);
    }
}
