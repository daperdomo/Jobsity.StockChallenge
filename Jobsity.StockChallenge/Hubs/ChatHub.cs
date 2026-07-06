using System.Security.Claims;
using Jobsity.StockChallenge.Application.Chat.Commands;
using Jobsity.StockChallenge.Application.Chat.Queries;
using Jobsity.StockChallenge.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Jobsity.StockChallenge.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IGetRecentChatMessagesHandler _getRecentChatMessagesHandler;
        private readonly ISendChatMessageHandler _sendChatMessageHandler;

        public ChatHub(
            IGetRecentChatMessagesHandler getRecentChatMessagesHandler,
            ISendChatMessageHandler sendChatMessageHandler)
        {
            _getRecentChatMessagesHandler = getRecentChatMessagesHandler;
            _sendChatMessageHandler = sendChatMessageHandler;
        }

        public override async Task OnConnectedAsync()
        {
            var history = await _getRecentChatMessagesHandler.HandleAsync(
                new GetRecentChatMessagesQuery(ChatRooms.General, 50));

            await Clients.Caller.SendAsync("LoadMessages", history);
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string message)
        {
            message = message.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userName = Context.User?.Identity?.Name ?? "User";
            var result = await _sendChatMessageHandler.HandleAsync(new SendChatMessageCommand(userId, userName, message));

            await Clients.All.SendAsync(
                "ReceiveMessage",
                result.Message.SenderUserName,
                result.Message.Message,
                result.Message.Timestamp);

            if (result.StockBotUnavailable)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "The stock bot is not available right now.", DateTime.UtcNow);
            }
        }
    }
}
