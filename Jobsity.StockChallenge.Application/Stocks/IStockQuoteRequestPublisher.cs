namespace Jobsity.StockChallenge.Application.Stocks
{
    public interface IStockQuoteRequestPublisher
    {
        Task RequestStockQuoteAsync(string stockSymbol, CancellationToken cancellationToken = default);
    }
}
