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

        public async Task<string> GetStockQuoteFromAlphaVantage(string stockSymbol)
        {
            if (string.IsNullOrWhiteSpace(stockSymbol))
                throw new ArgumentException("Stock symbol is required", nameof(stockSymbol));

            var cleanSymbol = stockSymbol.Trim().ToUpperInvariant();

            // Stooq usa AAPL.US, pero Alpha Vantage usa AAPL para acciones USA.
            if (cleanSymbol.EndsWith(".US", StringComparison.OrdinalIgnoreCase))
                cleanSymbol = cleanSymbol[..^3];

            var apiKey = _configuration["AlphaVantage:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Alpha Vantage API key is missing. Please add AlphaVantage:ApiKey to appsettings.json in the Bot project.");

            var serviceUrl = _configuration["AlphaVantage:QuoteUrl"]
                ?? "https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={stockSymbol}&apikey={apiKey}&datatype=csv";

            var url = serviceUrl
                .Replace("{stockSymbol}", Uri.EscapeDataString(cleanSymbol))
                .Replace("{apiKey}", Uri.EscapeDataString(apiKey));

            var csv = await FetchStringAsync(url);

            if (string.IsNullOrWhiteSpace(csv))
                return $"{cleanSymbol} quote is not available right now";

            if (csv.Contains("Error Message", StringComparison.OrdinalIgnoreCase) ||
                csv.Contains("Information", StringComparison.OrdinalIgnoreCase) ||
                csv.Contains("Thank you for using Alpha Vantage", StringComparison.OrdinalIgnoreCase) ||
                csv.Contains("<html", StringComparison.OrdinalIgnoreCase))
            {
                return $"{cleanSymbol} quote is not available right now";
            }

            var lines = csv.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (lines.Length < 2)
                return $"{cleanSymbol} quote is not available right now";

            // Expected Alpha Vantage GLOBAL_QUOTE CSV:
            // symbol,open,high,low,price,volume,latestDay,previousClose,change,changePercent
            var cols = lines[1].Split(',');

            if (cols.Length < 5)
                return $"{cleanSymbol} quote is not available right now";

            var symbol = cols[0].Trim().ToUpperInvariant();
            var price = cols[4].Trim();

            return string.IsNullOrWhiteSpace(price) || price.Equals("N/D", StringComparison.OrdinalIgnoreCase)
                ? $"{symbol} quote is not available right now"
                : $"{symbol} quote is ${price} per share";
        }

        protected virtual async Task<string> FetchStringAsync(string url)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            return await http.GetStringAsync(url);
        }
    }
}
