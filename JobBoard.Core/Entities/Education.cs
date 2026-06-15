using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{
    public class Education
    {
        public int Id { get; set; }
        public int CandidateProfileId { get; set; }
        public string School { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string Field { get; set; } = null!;
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }

        public CandidateProfile CandidateProfile { get; set; } = null!;
    }
}
