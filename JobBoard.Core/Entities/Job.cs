using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public string? Benefits { get; set; }
        public string Location { get; set; } = null!;
        public bool IsRemote { get; set; }
        public string JobType { get; set; } = null!;
        public string ExperienceLevel { get; set; } = null!;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? SalaryCurrency { get; set; }
        public string? SalaryPeriod { get; set; }
        public bool IsSalaryVisible { get; set; }
        public string Status { get; set; } = "active";
        public bool IsFeatured { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Deadline { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Company Company { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<JobSkill> RequiredSkills { get; set; } = new List<JobSkill>();
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public ICollection<SavedJob> SavedByUsers { get; set; } = new List<SavedJob>();
    }
}
