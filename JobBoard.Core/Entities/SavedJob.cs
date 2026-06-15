using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{

    public class SavedJob
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int JobId { get; set; }
        public DateTime SavedAt { get; set; }
        public User User { get; set; } = null!;
        public Job Job { get; set; } = null!;
    }

    public class JobAlert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public int? CategoryId { get; set; }
        public string? JobType { get; set; }
        public string Frequency { get; set; } = "daily";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}
