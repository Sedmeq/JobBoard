using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace JobBoard.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!; // "candidate" | "employer" | "admin"
        public string? AvatarUrl { get; set; }
        public string? Phone { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public bool IsDarkMode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public CandidateProfile? CandidateProfile { get; set; }
        public Company? Company { get; set; }
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public ICollection<JobAlert> JobAlerts { get; set; } = new List<JobAlert>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
