using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Companies;
using JobBoard.Core.DTOs.Jobs;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public CompanyService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<PagedResponse<CompanyListDto>> GetCompaniesAsync(CompanyFilterDto filter)
        {
            var query = _db.Companies
                .Include(c => c.Jobs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(kw) ||
                    (c.Description != null && c.Description.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Industry))
                query = query.Where(c => c.Industry == filter.Industry);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(c => c.Location != null &&
                    c.Location.ToLower().Contains(filter.Location.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Size))
                query = query.Where(c => c.CompanySize == filter.Size);

            if (filter.IsVerified.HasValue)
                query = query.Where(c => c.IsVerified == filter.IsVerified.Value);

            query = filter.SortBy switch
            {
                "name" => query.OrderBy(c => c.Name),
                "jobs_count" => query.OrderByDescending(c => c.Jobs.Count(j => j.Status == "active")),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CompanyListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    LogoUrl = c.LogoUrl,
                    Industry = c.Industry,
                    Location = c.Location,
                    CompanySize = c.CompanySize,
                    IsVerified = c.IsVerified,
                    ActiveJobCount = c.Jobs.Count(j => j.Status == "active"),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<CompanyListDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<CompanyDetailDto> GetCompanyByIdAsync(int id, int? currentUserId)
        {
            var company = await _db.Companies
                .Include(c => c.Jobs.Where(j => j.Status == "active"))
                    .ThenInclude(j => j.Category)
                .Include(c => c.Jobs)
                    .ThenInclude(j => j.Applications)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Şirkət tapılmadı.");

            var recentJobs = company.Jobs
                .Where(j => j.Status == "active")
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .Select(j => new JobListDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Slug = j.Slug,
                    Location = j.Location,
                    IsRemote = j.IsRemote,
                    JobType = j.JobType,
                    ExperienceLevel = j.ExperienceLevel,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    IsSalaryVisible = j.IsSalaryVisible,
                    IsFeatured = j.IsFeatured,
                    IsUrgent = j.IsUrgent,
                    Deadline = j.Deadline,
                    CreatedAt = j.CreatedAt,
                    ApplicationCount = j.Applications.Count,
                    Company = new JobCompanyDto
                    {
                        Id = company.Id,
                        Name = company.Name,
                        LogoUrl = company.LogoUrl,
                        IsVerified = company.IsVerified
                    },
                    Category = new JobCategoryDto
                    {
                        Id = j.Category.Id,
                        Name = j.Category.Name,
                        IconClass = j.Category.IconClass
                    }
                }).ToList();

            var avgRating = company.Reviews.Any()
                ? company.Reviews.Average(r => r.Rating)
                : 0;

            return new CompanyDetailDto
            {
                Id = company.Id,
                Name = company.Name,
                LogoUrl = company.LogoUrl,
                CoverImageUrl = company.CoverImageUrl,
                Description = company.Description,
                Industry = company.Industry,
                Location = company.Location,
                CompanySize = company.CompanySize,
                Website = company.Website,
                Phone = company.Phone,
                Email = company.Email,
                FoundedYear = company.FoundedYear,
                IsVerified = company.IsVerified,
                SocialFacebook = company.SocialFacebook,
                SocialTwitter = company.SocialTwitter,
                SocialLinkedIn = company.SocialLinkedIn,
                ActiveJobCount = company.Jobs.Count(j => j.Status == "active"),
                AverageRating = Math.Round(avgRating, 1),
                ReviewCount = company.Reviews.Count,
                CreatedAt = company.CreatedAt,
                RecentJobs = recentJobs
            };
        }

        public async Task<CompanyDetailDto> GetMyCompanyAsync(int userId)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət profili tapılmadı.");
            return await GetCompanyByIdAsync(company.Id, userId);
        }

        public async Task<CompanyDetailDto> UpdateMyCompanyAsync(int userId, CompanyUpdateDto dto)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət profili tapılmadı.");

            company.Name = dto.Name;
            company.Description = dto.Description;
            company.Industry = dto.Industry;
            company.CompanySize = dto.CompanySize;
            company.Website = dto.Website;
            company.Location = dto.Location;
            company.Phone = dto.Phone;
            company.Email = dto.Email;
            company.FoundedYear = dto.FoundedYear;
            company.SocialFacebook = dto.SocialFacebook;
            company.SocialTwitter = dto.SocialTwitter;
            company.SocialLinkedIn = dto.SocialLinkedIn;
            company.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return await GetCompanyByIdAsync(company.Id, userId);
        }

        public async Task<string> UploadLogoAsync(int userId, IFormFile file)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət tapılmadı.");

            var url = await SaveImageAsync(file, "logos", 200, 200);
            company.LogoUrl = url;
            company.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return url;
        }

        public async Task<string> UploadCoverAsync(int userId, IFormFile file)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət tapılmadı.");

            var url = await SaveImageAsync(file, "covers", 1200, 400);
            company.CoverImageUrl = url;
            company.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return url;
        }

        public async Task<PagedResponse<CompanyReviewDto>> GetReviewsAsync(int companyId, int page, int pageSize)
        {
            var query = _db.CompanyReviews
                .Include(r => r.User)
                .Where(r => r.CompanyId == companyId)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new CompanyReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Title = r.Title,
                    Pros = r.Pros,
                    Cons = r.Cons,
                    IsAnonymous = r.IsAnonymous,
                    ReviewerName = r.IsAnonymous ? "Anonim" : r.User.FullName,
                    ReviewerAvatar = r.IsAnonymous ? null : r.User.AvatarUrl,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<CompanyReviewDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task AddReviewAsync(int companyId, int userId, CompanyReviewCreateDto dto)
        {
            var company = await _db.Companies.FindAsync(companyId)
                ?? throw new NotFoundException("Şirkət tapılmadı.");

            if (dto.Rating < 1 || dto.Rating > 5)
                throw new BadRequestException("Reytinq 1-5 arasında olmalıdır.");

            var review = new CompanyReview
            {
                CompanyId = companyId,
                UserId = userId,
                Rating = dto.Rating,
                Title = dto.Title,
                Pros = dto.Pros,
                Cons = dto.Cons,
                IsAnonymous = dto.IsAnonymous,
                CreatedAt = DateTime.UtcNow
            };

            _db.CompanyReviews.Add(review);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<CompanyListDto>> GetFeaturedCompaniesAsync()
        {
            return await _db.Companies
                .Include(c => c.Jobs)
                .Where(c => c.IsVerified && c.LogoUrl != null)
                .OrderByDescending(c => c.Jobs.Count(j => j.Status == "active"))
                .Take(8)
                .Select(c => new CompanyListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    LogoUrl = c.LogoUrl,
                    Industry = c.Industry,
                    Location = c.Location,
                    CompanySize = c.CompanySize,
                    IsVerified = c.IsVerified,
                    ActiveJobCount = c.Jobs.Count(j => j.Status == "active"),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<string> SaveImageAsync(IFormFile file, string folder, int width, int height)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new BadRequestException("Yalnız JPG, PNG və WebP formatları qəbul edilir.");

            if (file.Length > 10 * 1024 * 1024)
                throw new BadRequestException("Fayl ölçüsü 10MB-dan çox ola bilməz.");

            var uploadsPath = Path.Combine("wwwroot", "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = _config["Storage:BaseUrl"];
            return $"{baseUrl}/uploads/{folder}/{fileName}";
        }
    }
}
