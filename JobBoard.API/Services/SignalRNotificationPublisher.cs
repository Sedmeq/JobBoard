using JobBoard.API.Hubs;
using JobBoard.Core.DTOs.Alerts;
using JobBoard.Core.DTOs.Chat;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace JobBoard.API.Services
{
    /// <summary>
    /// INotificationPublisher-in SignalR ilə konkret implementasiyası.
    /// </summary>
    public class SignalRNotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hub;

        public SignalRNotificationPublisher(IHubContext<NotificationHub> hub) => _hub = hub;

        public Task PushToUserAsync(int userId, NotificationDto notification) =>
            _hub.Clients.Group(NotificationHub.UserGroup(userId))
                .SendAsync("ReceiveNotification", notification);

        public Task PushUnreadCountAsync(int userId, int unreadCount) =>
            _hub.Clients.Group(NotificationHub.UserGroup(userId))
                .SendAsync("UnreadCount", unreadCount);

        public Task PushChatMessageAsync(int userId, ChatMessageDto message) =>
            _hub.Clients.Group(NotificationHub.UserGroup(userId))
                .SendAsync("ReceiveChatMessage", message);

        public Task PushChatClosedAsync(int userId, int conversationId) =>
            _hub.Clients.Group(NotificationHub.UserGroup(userId))
                .SendAsync("ChatClosed", conversationId);
    }
}
