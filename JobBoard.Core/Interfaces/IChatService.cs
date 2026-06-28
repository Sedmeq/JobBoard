using JobBoard.Core.DTOs.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    public interface IChatService
    {
        /// <summary>İşəgötürən bir müraciətə cavab olaraq söhbət başladır (və ya mövcudu qaytarır).</summary>
        Task<ChatConversationDto> StartConversationAsync(int employerUserId, int applicationId);

        /// <summary>Cari istifadəçinin bütün söhbətləri.</summary>
        Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(int userId);

        /// <summary>Söhbət detalları + mesajlar (yalnız iştirakçı baxa bilər). Qarşı tərəfin mesajları oxunmuş işarələnir.</summary>
        Task<ChatConversationDetailDto> GetConversationAsync(int userId, int conversationId);

        /// <summary>Söhbətə mesaj göndərir.</summary>
        Task<ChatMessageDto> SendMessageAsync(int userId, int conversationId, string content);

        /// <summary>İşəgötürən söhbəti bağlayır.</summary>
        Task CloseConversationAsync(int employerUserId, int conversationId);
    }
}
