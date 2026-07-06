using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jobsity.StockChallenge.Bot.Services
{
    public class StockService : IStockService
    {
        private readonly IConfiguration _configuration;

        public StockService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetStockQuote(string stockSymbol)
        {
            string serviceUrl = _configuration["StockService:Url"] ?? "https://stooq.com/q/l/?s={stockSymbol}&f=sd2t2ohlcv&h&e=csv";
            var url = serviceUrl.Replace("{stockSymbol}", stockSymbol);
            var csv = await FetchStringAsync(url);

            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) throw new InvalidOperationException("CSV response missing data");

            var cols = lines[1].Split(',');
            var symbol = cols[0].ToUpperInvariant();
            var close = cols[6];

            string message = close.Equals("N/D", StringComparison.OrdinalIgnoreCase)
                ? $"{symbol} quote is not available right now"
                : $"{symbol} quote is ${close} per share";
            return message;
        }

        // Extracted for unit testing so tests can override the HTTP call.
        protected virtual async Task<string> FetchStringAsync(string url)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            return await http.GetStringAsync(url);
        }
    }
}
