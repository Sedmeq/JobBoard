using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{

    public class WorkExperience
    {
        public int Id { get; set; }
        public int CandidateProfileId { get; set; }
        public string Company { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string? Location { get; set; }
        public string StartDate { get; set; } = null!;
        public string? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }

        public CandidateProfile CandidateProfile { get; set; } = null!;
    }
}
