using System;
using System.Collections.Generic;

namespace JobBoard.Core.DTOs.Chat
{
    public class StartChatDto
    {
        public int ApplicationId { get; set; }
    }

    public class SendMessageDto
    {
        public string Content { get; set; } = null!;
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderUserId { get; set; }
        public string SenderName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class ChatConversationDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }

        // Qarşı tərəf (cari istifadəçiyə görə)
        public int OtherPartyUserId { get; set; }
        public string OtherPartyName { get; set; } = null!;
        public string? OtherPartyAvatar { get; set; }

        public string? LastMessage { get; set; }
        public int UnreadCount { get; set; }

        // Cari istifadəçinin söhbətdəki rolu
        public bool IsEmployer { get; set; }
    }

    public class ChatConversationDetailDto : ChatConversationDto
    {
        public List<ChatMessageDto> Messages { get; set; } = [];
    }
}
