using Jobsity.StockChallenge.Application.Common;
using Jobsity.StockChallenge.Application.Stocks;
using Jobsity.StockChallenge.Domain.Entities;

namespace Jobsity.StockChallenge.Application.Chat.Commands
{
    public class SendChatMessageHandler : ISendChatMessageHandler
    {
        private const string StockCommandPrefix = "/stock=";
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IStockQuoteRequestPublisher _stockQuoteRequestPublisher;

        public SendChatMessageHandler(
            IChatMessageRepository chatMessageRepository,
            IStockQuoteRequestPublisher stockQuoteRequestPublisher)
        {
            _chatMessageRepository = chatMessageRepository;
            _stockQuoteRequestPublisher = stockQuoteRequestPublisher;
        }

        public async Task<SendChatMessageResult> HandleAsync(SendChatMessageCommand command, CancellationToken cancellationToken = default)
        {
            var text = command.Message.Trim();
            var chatRoom = ChatRooms.Normalize(command.ChatRoom);
            var isStockCommand = IsStockCommand(text);
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = command.SenderId,
                SenderUserName = command.SenderUserName,
                Message = text,
                Timestamp = DateTime.UtcNow,
                ChatRoom = chatRoom
            };

            if (!isStockCommand)
            {
                await _chatMessageRepository.AddAsync(chatMessage, cancellationToken);
            }

            var stockBotUnavailable = false;
            if (isStockCommand)
            {
                var stockSymbol = text[StockCommandPrefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(stockSymbol))
                {
                    try
                    {
                        await _stockQuoteRequestPublisher.RequestStockQuoteAsync(stockSymbol, chatRoom, cancellationToken);
                    }
                    catch
                    {
                        stockBotUnavailable = true;
                    }
                }
            }

            return new SendChatMessageResult(ToDto(chatMessage), stockBotUnavailable);
        }

        private static bool IsStockCommand(string text)
        {
            return text.StartsWith(StockCommandPrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static ChatMessageDto ToDto(ChatMessage message)
        {
            return new ChatMessageDto(message.SenderUserName, message.Message, message.Timestamp, message.ChatRoom);
        }
    }
}
