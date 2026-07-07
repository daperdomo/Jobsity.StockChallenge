namespace Jobsity.StockChallenge.Application.Stocks
{
    public interface IStockQuoteRequestPublisher
    {
        Task RequestStockQuoteAsync(string stockSymbol, string chatRoom, CancellationToken cancellationToken = default);
    }
}
