namespace Jobsity.StockChallenge.Application.Chat
{
    public record ChatMessageDto(string SenderUserName, string Message, DateTime Timestamp, string ChatRoom);
}
