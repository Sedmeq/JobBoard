using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{

    public class CandidateProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
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
        public string? ResumeUrl { get; set; }
        public string? VideoResumeUrl { get; set; }

        public User User { get; set; } = null!;
        public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
        public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
        public ICollection<Education> Educations { get; set; } = new List<Education>();
        public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
        public ICollection<CandidateLanguage> Languages { get; set; } = new List<CandidateLanguage>();
    }
}
