using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{

    public class Portfolio
    {
        public int Id { get; set; }
        public int CandidateProfileId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string[]? Technologies { get; set; }
        public string? Category { get; set; }
        public string? CompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;
    }
}
