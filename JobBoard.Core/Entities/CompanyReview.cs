using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{

    public class CompanyReview
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Pros { get; set; }
        public string? Cons { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }
        public Company Company { get; set; } = null!;
        public User User { get; set; } = null!;
    }

    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NewsletterSubscriber
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
        public string UnsubscribeToken { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime SubscribedAt { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!; // "application_status" | "job_alert" | "system"
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}
