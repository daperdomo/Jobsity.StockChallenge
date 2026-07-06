namespace Jobsity.StockChallenge.Bot.Services
{
    public interface IStockService
    {
        Task<string> GetStockQuote(string stockSymbol);
    }
}
