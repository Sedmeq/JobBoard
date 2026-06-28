using JobBoard.Core.DTOs.Alerts;
using JobBoard.Core.DTOs.Chat;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    /// <summary>
    /// Real-time bildiriş yayımı üçün abstraksiya (SignalR ilə implement olunur).
    /// Infrastructure qatı bu interfeysə bağlıdır, konkret SignalR implementasiyası API qatındadır.
    /// </summary>
    public interface INotificationPublisher
    {
        /// <summary>Konkret istifadəçiyə real-time bildiriş göndərir.</summary>
        Task PushToUserAsync(int userId, NotificationDto notification);

        /// <summary>Oxunmamış bildiriş sayını real-time yeniləyir.</summary>
        Task PushUnreadCountAsync(int userId, int unreadCount);

        /// <summary>Söhbət mesajını real-time olaraq istifadəçiyə göndərir.</summary>
        Task PushChatMessageAsync(int userId, ChatMessageDto message);

        /// <summary>Söhbətin bağlandığını real-time bildirir.</summary>
        Task PushChatClosedAsync(int userId, int conversationId);
    }
}
