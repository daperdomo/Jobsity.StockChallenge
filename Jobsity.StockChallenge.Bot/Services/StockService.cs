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

        public async Task<string> GetStockQuoteFromHistory(string stockSymbol)
        {
            if (string.IsNullOrWhiteSpace(stockSymbol))
                throw new ArgumentException("Stock symbol is required", nameof(stockSymbol));

            var cleanSymbol = stockSymbol.Trim().ToLowerInvariant();
            var displaySymbol = cleanSymbol.ToUpperInvariant();

            var toDate = DateTime.UtcNow.Date;
            var fromDate = toDate.AddDays(-7);

            var d1 = fromDate.ToString("yyyyMMdd");
            var d2 = toDate.ToString("yyyyMMdd");

            string serviceUrl = _configuration["StockService:HistoryUrl"]
                ?? "https://stooq.com/q/d/l/?s={stockSymbol}&i=d&d1={d1}&d2={d2}";

            var url = serviceUrl
                .Replace("{stockSymbol}", Uri.EscapeDataString(cleanSymbol))
                .Replace("{d1}", d1)
                .Replace("{d2}", d2);

            var csv = await FetchStringAsync(url);

            if (string.IsNullOrWhiteSpace(csv))
                return $"{displaySymbol} quote is not available right now";

            if (csv.Contains("Access denied", StringComparison.OrdinalIgnoreCase) ||
                csv.Contains("<html", StringComparison.OrdinalIgnoreCase))
            {
                return $"{displaySymbol} quote is not available right now";
            }

            var lines = csv.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (lines.Length < 2)
                return $"{displaySymbol} quote is not available right now";

            // Historical CSV format:
            // Date,Open,High,Low,Close,Volume
            var lastRecord = lines[^1];
            var cols = lastRecord.Split(',');

            if (cols.Length < 5)
                return $"{displaySymbol} quote is not available right now";

            var date = cols[0].Trim();
            var close = cols[4].Trim();

            string message = close.Equals("N/D", StringComparison.OrdinalIgnoreCase) ||
                             string.IsNullOrWhiteSpace(close)
                ? $"{displaySymbol} quote is not available right now"
                : $"{displaySymbol} quote is ${close} per share";

            return message;
        }

        protected virtual async Task<string> FetchStringAsync(string url)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            return await http.GetStringAsync(url);
        }
    }
}
