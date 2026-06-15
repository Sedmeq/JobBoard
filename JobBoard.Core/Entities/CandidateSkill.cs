using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{
    public class CandidateSkill
    {
        public int Id { get; set; }
        public int CandidateProfileId { get; set; }
        public string Name { get; set; } = null!;
        public string? Level { get; set; } // "beginner" | "intermediate" | "expert"
        public CandidateProfile CandidateProfile { get; set; } = null!;
    }

    public class JobSkill
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string Name { get; set; } = null!;
        public Job Job { get; set; } = null!;
    }

    public class CandidateLanguage
    {
        public int Id { get; set; }
        public int CandidateProfileId { get; set; }
        public string Name { get; set; } = null!;
        public string Level { get; set; } = null!; // "basic" | "intermediate" | "fluent" | "native"
        public CandidateProfile CandidateProfile { get; set; } = null!;
    }
}
