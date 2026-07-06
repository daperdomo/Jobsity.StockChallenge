namespace Jobsity.StockChallenge.Application.Chat.Commands
{
    public record SendChatMessageCommand(string SenderId, string SenderUserName, string Message);
}
