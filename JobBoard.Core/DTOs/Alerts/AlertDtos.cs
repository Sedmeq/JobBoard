using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Alerts
{

    public class AlertCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public int? CategoryId { get; set; }
        public string? JobType { get; set; }
        public string Frequency { get; set; } = "daily";
    }

    public class AlertUpdateDto : AlertCreateDto { }

    public class AlertToggleDto
    {
        public bool IsActive { get; set; }
    }

    public class AlertDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public int? CategoryId { get; set; }
        public string? JobType { get; set; }
        public string Frequency { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SavedJobDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public DateTime SavedAt { get; set; }
        public JobBoard.Core.DTOs.Jobs.JobListDto Job { get; set; } = null!;
    }
}
