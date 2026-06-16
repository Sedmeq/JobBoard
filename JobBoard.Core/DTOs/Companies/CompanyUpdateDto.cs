using JobBoard.Core.DTOs.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Companies
{

    public class CompanyUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? FoundedYear { get; set; }
        public string? SocialFacebook { get; set; }
        public string? SocialTwitter { get; set; }
        public string? SocialLinkedIn { get; set; }
    }

    public class CompanyFilterDto
    {
        public string? Keyword { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string? Size { get; set; }
        public bool? IsVerified { get; set; }
        public string? SortBy { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CompanyListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string? CompanySize { get; set; }
        public bool IsVerified { get; set; }
        public int ActiveJobCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CompanyDetailDto : CompanyListDto
    {
        public string? CoverImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? FoundedYear { get; set; }
        public string? SocialFacebook { get; set; }
        public string? SocialTwitter { get; set; }
        public string? SocialLinkedIn { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFollowing { get; set; }
        public List<JobListDto> RecentJobs { get; set; } = [];
    }

    public class CompanyReviewCreateDto
    {
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Pros { get; set; }
        public string? Cons { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class CompanyReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Pros { get; set; }
        public string? Cons { get; set; }
        public bool IsAnonymous { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
