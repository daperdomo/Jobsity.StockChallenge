using Jobsity.StockChallenge.Bot.Services;

namespace Jobsity.StockChallenge.Tests
{
    public class StockServiceTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetStockQuote_ReturnsPriceMessage_WhenCsvHasValue()
        {
            var svc = new TestableStockService("symbol,Date,Time,Open,High,Low,Close,Volume\nMSFT.US,2023-01-01,00:00:00,1,2,3,123.45,1000");
            var msg = await svc.GetStockQuote("msft.us");
            Assert.That(msg, Is.EqualTo("MSFT.US quote is $123.45 per share"));
        }

        [Test]
        public async Task GetStockQuote_ReturnsNotAvailable_WhenCsvHasND()
        {
            var svc = new TestableStockService("symbol,Date,Time,Open,High,Low,Close,Volume\nAAPL.US,2023-01-01,00:00:00,1,2,3,N/D,1000");
            var msg = await svc.GetStockQuote("aapl.us");
            Assert.That(msg, Is.EqualTo("AAPL.US quote is not available right now"));
        }

        [Test]
        public void GetStockQuote_Throws_WhenCsvMissingData()
        {
            var svc = new TestableStockService("onlyheader\n");
            Assert.ThrowsAsync<InvalidOperationException>(async () => await svc.GetStockQuote("x"));
        }

        private class TestableStockService : StockService
        {
            private readonly string _response;

            public TestableStockService(string response) : base(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build())
            {
                _response = response;
            }

            protected override Task<string> FetchStringAsync(string url)
            {
                return Task.FromResult(_response);
            }
        }
    }
}
