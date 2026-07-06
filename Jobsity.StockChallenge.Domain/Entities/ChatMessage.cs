namespace Jobsity.StockChallenge.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public string SenderUserName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public string ChatRoom { get; set; } = "General";
    }
}
