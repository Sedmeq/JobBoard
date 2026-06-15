using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int UserId { get; set; }
        public string CoverLetter { get; set; } = null!;
        public string? ResumeUrl { get; set; }
        public string Status { get; set; } = "pending";
        public string? EmployerNote { get; set; }
        public DateTime? InterviewDate { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Job Job { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
