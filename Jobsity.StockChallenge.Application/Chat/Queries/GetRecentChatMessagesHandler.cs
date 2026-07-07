namespace Jobsity.StockChallenge.Application.Chat.Queries
{
    public class GetRecentChatMessagesHandler : IGetRecentChatMessagesHandler
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public GetRecentChatMessagesHandler(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<IReadOnlyList<ChatMessageDto>> HandleAsync(GetRecentChatMessagesQuery query, CancellationToken cancellationToken = default)
        {
            var messages = await _chatMessageRepository.GetRecentAsync(query.ChatRoom, query.Count, cancellationToken);
            return messages
                .Select(message => new ChatMessageDto(message.SenderUserName, message.Message, message.Timestamp, message.ChatRoom))
                .ToList();
        }
    }
}
