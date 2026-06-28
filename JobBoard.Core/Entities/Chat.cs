using System;
using System.Collections.Generic;

namespace JobBoard.Core.Entities
{
    /// <summary>
    /// İşəgötürən ilə namizəd arasında bir müraciət üzrə söhbət.
    /// </summary>
    public class ChatConversation
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public int EmployerUserId { get; set; }
        public int CandidateUserId { get; set; }
        public string Status { get; set; } = "open"; // open | closed
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public List<ChatMessage> Messages { get; set; } = [];
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderUserId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        public ChatConversation Conversation { get; set; } = null!;
    }
}
