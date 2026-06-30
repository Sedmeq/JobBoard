using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Admin
{
    public class AdminDtos
    {
    }

    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int TotalCompanies { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewJobsThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public Dictionary<string, int> JobsByStatus { get; set; } = [];
        public List<TopCategoryDto> TopCategories { get; set; } = [];
    }

    public class TopCategoryDto
    {
        public string Name { get; set; } = null!;
        public int JobCount { get; set; }
    }

    public class AdminUserListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BannedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminUserStatusDto
    {
        public bool IsActive { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>İstifadəçini ban etmək / banı götürmək üçün.</summary>
    public class AdminBanUserDto
    {
        public bool IsBanned { get; set; }
        public string? Reason { get; set; }
    }

    public class AdminVerifyCompanyDto
    {
        public bool IsVerified { get; set; }
    }

    public class AdminCompanyListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public int JobsCount { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminJobListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? CompanyName { get; set; }
        public string? CategoryName { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = null!;
        public int ApplicationCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminTransactionListDto
    {
        public int Id { get; set; }
        public string? OrderId { get; set; }
        public string TransactionType { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? UserFullName { get; set; }
    }

    public class ContactMessageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsReplied { get; set; }
        public string? ReplyMessage { get; set; }
        public DateTime? RepliedAt { get; set; }
    }

    public class ContactCreateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Phone { get; set; }
        public string? RecaptchaToken { get; set; }
    }

    public class ContactReplyDto
    {
        public string Message { get; set; } = null!;
    }

    /// <summary>Açar-dəyər prinsipi ilə əlaqə məlumatı (Address, Email, Phone və s.).</summary>
    public class ContactInfoItemDto
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    /// <summary>Contact səhifəsi üçün public məlumatlar (əlaqə + xəritə + reCAPTCHA site key).</summary>
    public class ContactPublicInfoDto
    {
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? WorkingHours { get; set; }
        public string? MapEmbedUrl { get; set; }
        public bool RecaptchaEnabled { get; set; }
        public string? RecaptchaSiteKey { get; set; }
    }

    /// <summary>Admin paneldən idarə olunan sayt parametrləri.</summary>
    public class SiteSettingsDto
    {
        public string? ContactAddress { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactWorkingHours { get; set; }
        public string? MapEmbedUrl { get; set; }
        public bool RecaptchaEnabled { get; set; }
        public string? RecaptchaSiteKey { get; set; }
    }

    public class NewsletterSubscribeDto
    {
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
    }

    public class NewsletterUnsubscribeDto
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
    }

    public class PublicStatsDto
    {
        public int TotalJobs { get; set; }
        public int TotalCandidates { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalHired { get; set; }
        public int NewJobsThisWeek { get; set; }
        public List<string> TopLocations { get; set; } = [];
        public List<string> TopSkills { get; set; } = [];
    }

    public class GlobalSearchDto
    {
        public List<SearchJobDto> Jobs { get; set; } = [];
        public List<SearchCompanyDto> Companies { get; set; } = [];
        public List<SearchCandidateDto> Candidates { get; set; } = [];
        public List<SearchBlogDto> BlogPosts { get; set; } = [];
    }

    public class SearchJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Company { get; set; } = null!;
        public string Location { get; set; } = null!;
    }

    public class SearchCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? Industry { get; set; }
    }

    public class SearchCandidateDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Headline { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class SearchBlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Excerpt { get; set; }
    }
}
