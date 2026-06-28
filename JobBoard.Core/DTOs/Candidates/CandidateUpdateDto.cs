using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Candidates
{

    public class CandidateUpdateDto
    {
        public string? Headline { get; set; }
        public string? Summary { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GithubUrl { get; set; }
        public int? ExperienceYears { get; set; }
        public string? CurrentPosition { get; set; }
        public string? ExpectedSalary { get; set; }
        public bool IsAvailable { get; set; }
        public List<string> Skills { get; set; } = [];
        public List<CandidateLanguageDto> Languages { get; set; } = [];
    }

    public class CandidateLanguageDto
    {
        public string Name { get; set; } = null!;
        public string Level { get; set; } = null!;
    }

    public class WorkExperienceCreateDto
    {
        public string Company { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string? Location { get; set; }
        public string StartDate { get; set; } = null!;
        public string? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }

    public class EducationCreateDto
    {
        public string School { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string Field { get; set; } = null!;
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }

    public class CandidateFilterDto
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? Skills { get; set; }
        public int? ExperienceMin { get; set; }
        public int? ExperienceMax { get; set; }
        public bool? IsAvailable { get; set; }
        public string? SortBy { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class WorkExperienceDto
    {
        public int Id { get; set; }
        public string Company { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string? Location { get; set; }
        public string StartDate { get; set; } = null!;
        public string? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }

    public class EducationDto
    {
        public int Id { get; set; }
        public string School { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string Field { get; set; } = null!;
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }

    public class CandidateListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Headline { get; set; }
        public string? Location { get; set; }
        public int? ExperienceYears { get; set; }
        public bool IsAvailable { get; set; }
        public List<string> Skills { get; set; } = [];
        public DateTime CreatedAt { get; set; }
    }

    public class CandidateDetailDto : CandidateListDto
    {
        public string? Email { get; set; }
        public string? Summary { get; set; }
        public string? Website { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? CurrentPosition { get; set; }
        public string? ExpectedSalary { get; set; }
        public string? ResumeUrl { get; set; }
        public string? VideoResumeUrl { get; set; }
        public List<WorkExperienceDto> WorkExperiences { get; set; } = [];
        public List<EducationDto> Educations { get; set; } = [];
        public List<CandidateLanguageDto> Languages { get; set; } = [];
    }
}
