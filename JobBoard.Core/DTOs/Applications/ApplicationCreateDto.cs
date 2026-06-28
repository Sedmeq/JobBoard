using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Applications
{

    public class ApplicationCreateDto
    {
        public int JobId { get; set; }
        public string CoverLetter { get; set; } = null!;
        public bool UseProfileResume { get; set; } = true;
        public string? ResumeUrl { get; set; }
    }

    public class ApplicationStatusUpdateDto
    {
        public string Status { get; set; } = null!;
        public string? EmployerNote { get; set; }
        public DateTime? InterviewDate { get; set; }
    }

    public class ApplicationFilterDto
    {
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ApplicationJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string JobType { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? CompanyLogo { get; set; }
    }

    public class ApplicationCandidateDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Headline { get; set; }
        public string? Location { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ResumeUrl { get; set; }
    }

    public class ApplicationListDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public string CoverLetter { get; set; } = null!;
        public string? ResumeUrl { get; set; }
        public string? EmployerNote { get; set; }
        public DateTime? InterviewDate { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ApplicationJobDto Job { get; set; } = null!;
        public ApplicationCandidateDto? Candidate { get; set; }
    }

    public class ApplicationStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Reviewing { get; set; }
        public int Shortlisted { get; set; }
        public int Interview { get; set; }
        public int Offered { get; set; }
        public int Rejected { get; set; }
        public int Withdrawn { get; set; }
    }

    public class CompanyApplicantDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Headline { get; set; }
        public string? Location { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ResumeUrl { get; set; }
        public List<string> Skills { get; set; } = [];
        public int AppliedJobsCount { get; set; }
        public DateTime LastAppliedAt { get; set; }
    }
}
