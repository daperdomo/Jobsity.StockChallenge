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

        public async Task JoinRoom(string chatRoom)
        {
            var normalizedRoom = ChatRooms.Normalize(chatRoom);
            if (Context.Items.TryGetValue("ChatRoom", out var currentRoom) && currentRoom is string previousRoom)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, previousRoom);
            }

            Context.Items["ChatRoom"] = normalizedRoom;
            await Groups.AddToGroupAsync(Context.ConnectionId, normalizedRoom);

            var history = await _getRecentChatMessagesHandler.HandleAsync(
                new GetRecentChatMessagesQuery(normalizedRoom, 50));

            await Clients.Caller.SendAsync("LoadMessages", normalizedRoom, history);
        }

        public async Task SendMessage(string message, string chatRoom)
        {
            message = message.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userName = Context.User?.Identity?.Name ?? "User";
            var normalizedRoom = ChatRooms.Normalize(chatRoom);
            var result = await _sendChatMessageHandler.HandleAsync(new SendChatMessageCommand(userId, userName, message, normalizedRoom));

            await Clients.Group(normalizedRoom).SendAsync(
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
