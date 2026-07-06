using Jobsity.StockChallenge.Application.Chat;
using Jobsity.StockChallenge.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Jobsity.StockChallenge.Services
{
    public class SignalRChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRChatNotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyMessageAsync(ChatMessageDto message, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceiveMessage",
                message.SenderUserName,
                message.Message,
                message.Timestamp,
                cancellationToken);
        }
    }
}
