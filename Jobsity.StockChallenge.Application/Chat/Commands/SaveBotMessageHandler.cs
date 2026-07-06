using Jobsity.StockChallenge.Application.Common;
using Jobsity.StockChallenge.Domain.Entities;

namespace Jobsity.StockChallenge.Application.Chat.Commands
{
    public class SaveBotMessageHandler : ISaveBotMessageHandler
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public SaveBotMessageHandler(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<ChatMessageDto> HandleAsync(SaveBotMessageCommand command, CancellationToken cancellationToken = default)
        {
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = "stock-bot",
                SenderUserName = "Stock Bot",
                Message = command.Message,
                Timestamp = DateTime.UtcNow,
                ChatRoom = ChatRooms.General
            };

            return new ChatMessageDto(chatMessage.SenderUserName, chatMessage.Message, chatMessage.Timestamp);
        }
    }
}
