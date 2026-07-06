using Jobsity.StockChallenge.Application.Chat;
using Jobsity.StockChallenge.Application.Chat.Commands;
using Jobsity.StockChallenge.Application.Stocks;
using Jobsity.StockChallenge.Domain.Entities;

namespace Jobsity.StockChallenge.Tests
{
    public class ChatMessageHandlingTests
    {
        [Test]
        public async Task SendChatMessageHandler_DoesNotPersistStockCommands_ButReturnsMessageForDisplay()
        {
            var repository = new FakeChatMessageRepository();
            var publisher = new FakeStockQuotePublisher();
            var handler = new SendChatMessageHandler(repository, publisher);

            var result = await handler.HandleAsync(new SendChatMessageCommand("user-1", "Alice", "/stock=MSFT.US"));

            Assert.That(result.Message.Message, Is.EqualTo("/stock=MSFT.US"));
            Assert.That(repository.AddedMessages, Is.Empty);
            Assert.That(publisher.RequestedSymbols, Is.EqualTo(new[] { "MSFT.US" }));
        }

        [Test]
        public async Task SaveBotMessageHandler_DoesNotPersistBotResponses_ButReturnsMessageForDisplay()
        {
            var repository = new FakeChatMessageRepository();
            var handler = new SaveBotMessageHandler(repository);

            var result = await handler.HandleAsync(new SaveBotMessageCommand("MSFT.US quote is $123.45 per share"));

            Assert.That(result.Message, Is.EqualTo("MSFT.US quote is $123.45 per share"));
            Assert.That(repository.AddedMessages, Is.Empty);
        }

        private sealed class FakeChatMessageRepository : IChatMessageRepository
        {
            public List<ChatMessage> AddedMessages { get; } = new();

            public Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
            {
                AddedMessages.Add(message);
                return Task.CompletedTask;
            }

            public Task<IReadOnlyList<ChatMessage>> GetRecentAsync(string chatRoom, int count, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<ChatMessage>>(Array.Empty<ChatMessage>());
            }
        }

        private sealed class FakeStockQuotePublisher : IStockQuoteRequestPublisher
        {
            public List<string> RequestedSymbols { get; } = new();

            public Task RequestStockQuoteAsync(string symbol, CancellationToken cancellationToken = default)
            {
                RequestedSymbols.Add(symbol);
                return Task.CompletedTask;
            }
        }
    }
}
