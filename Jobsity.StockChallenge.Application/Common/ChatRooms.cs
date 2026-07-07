namespace Jobsity.StockChallenge.Application.Common
{
    public static class ChatRooms
    {
        public const string General = "General";
        public const string Stocks = "Stocks";
        public const string Random = "Random";

        public static readonly IReadOnlyList<string> All = new[]
        {
            General,
            Stocks,
            Random
        };

        public static string Normalize(string? chatRoom)
        {
            return All.FirstOrDefault(room => string.Equals(room, chatRoom, StringComparison.OrdinalIgnoreCase))
                ?? General;
        }
    }
}
