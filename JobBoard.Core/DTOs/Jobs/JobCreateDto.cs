using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Jobs
{

    public class JobCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public string? Benefits { get; set; }
        public string Location { get; set; } = null!;
        public bool IsRemote { get; set; }
        public string JobType { get; set; } = null!;
        public string ExperienceLevel { get; set; } = null!;
        public int CategoryId { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? SalaryCurrency { get; set; }
        public string? SalaryPeriod { get; set; }
        public bool IsSalaryVisible { get; set; }
        public bool IsUrgent { get; set; }
        public DateTime Deadline { get; set; }
        public List<string> RequiredSkills { get; set; } = [];
    }

    public class JobUpdateDto : JobCreateDto { }

    public class JobStatusDto
    {
        public string Status { get; set; } = null!;
    }

    public class JobFeaturedDto
    {
        public bool IsFeatured { get; set; }
    }

    public class JobFilterDto
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public int? CategoryId { get; set; }
        public string? JobType { get; set; }
        public string? ExperienceLevel { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool? IsRemote { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsUrgent { get; set; }
        public string? Status { get; set; } = "active";
        public int? CompanyId { get; set; }
        public string? SortBy { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class JobCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public bool IsVerified { get; set; }
        public string? Location { get; set; }
    }

    public class JobCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IconClass { get; set; }
    }

    public class JobListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Location { get; set; } = null!;
        public bool IsRemote { get; set; }
        public string JobType { get; set; } = null!;
        public string ExperienceLevel { get; set; } = null!;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? SalaryCurrency { get; set; }
        public bool IsSalaryVisible { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsUrgent { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public int ApplicationCount { get; set; }
        public string? Status { get; set; }
        public JobCompanyDto Company { get; set; } = null!;
        public JobCategoryDto Category { get; set; } = null!;
    }

    public class JobDetailDto : JobListDto
    {
        public string Description { get; set; } = null!;
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public string? Benefits { get; set; }
        public string? SalaryPeriod { get; set; }
        public List<string> RequiredSkills { get; set; } = [];
        public bool IsSaved { get; set; }
        public bool HasApplied { get; set; }
    }
}
