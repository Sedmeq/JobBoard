using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{

    public class Company
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? FoundedYear { get; set; }
        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; }
        public string? SocialFacebook { get; set; }
        public string? SocialTwitter { get; set; }
        public string? SocialLinkedIn { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        public ICollection<CompanyReview> Reviews { get; set; } = new List<CompanyReview>();
    }
}
