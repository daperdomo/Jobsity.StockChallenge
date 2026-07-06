using Jobsity.StockChallenge.Application.Chat;

namespace Jobsity.StockChallenge.Application.Chat.Commands
{
    public record SendChatMessageResult(ChatMessageDto Message, bool StockBotUnavailable);
}
