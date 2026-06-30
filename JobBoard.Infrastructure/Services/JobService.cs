using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Jobs;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class JobService : IJobService
    {
        private readonly AppDbContext _db;
        private readonly IConnectionMultiplexer? _redis;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public JobService(
            AppDbContext db,
            INotificationService notificationService,
            IEmailService emailService,
            IConnectionMultiplexer? redis = null)
        {
            _db = db;
            _notificationService = notificationService;
            _emailService = emailService;
            _redis = redis;
        }

        private async Task NotifyMatchingAlertsAsync(Job job, string companyName, List<string> skills)
        {
            var alerts = await _db.JobAlerts
                .Include(a => a.User)
                .Where(a => a.IsActive)
                .ToListAsync();

            var titleLower = (job.Title ?? "").ToLowerInvariant();
            var descLower = (job.Description ?? "").ToLowerInvariant();
            var locLower = (job.Location ?? "").ToLowerInvariant();
            var skillsLower = (skills ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.ToLowerInvariant())
                .ToList();
            var notified = new HashSet<int>();

            foreach (var a in alerts)
            {
                if (a.CategoryId.HasValue && a.CategoryId.Value != job.CategoryId) continue;
                if (!string.IsNullOrWhiteSpace(a.JobType) &&
                    !string.Equals(a.JobType, job.JobType, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrWhiteSpace(a.Location) &&
                    !locLower.Contains(a.Location.ToLowerInvariant())) continue;

                // Mətn meyarları: alert adı (Name) və keyword (varsa) — hər ikisi nəzərə alınır.
                // Yalnız Name varsa Name-ə görə, həm Name həm keyword varsa hər ikisi uyğun olmalıdır.
                var terms = new List<string>();
                if (!string.IsNullOrWhiteSpace(a.Name)) terms.Add(a.Name.Trim().ToLowerInvariant());
                if (!string.IsNullOrWhiteSpace(a.Keyword)) terms.Add(a.Keyword.Trim().ToLowerInvariant());
                if (terms.Count > 0 &&
                    !terms.All(term => titleLower.Contains(term)
                        || descLower.Contains(term)
                        || skillsLower.Any(sk => sk.Contains(term))))
                    continue;

                if (!notified.Add(a.UserId)) continue; // hər istifadəçiyə bir dəfə

                await _notificationService.CreateNotificationAsync(
                    a.UserId,
                    "Yeni uyğun vakansiya",
                    $"\"{job.Title}\" ({companyName}) sizin iş bildirişinizə uyğundur.",
                    "job_alert",
                    $"job-detail.html?id={job.Id}");

                if (a.User != null && !string.IsNullOrWhiteSpace(a.User.Email))
                {
                    try { await _emailService.SendJobAlertMatchAsync(a.User.Email, a.User.FullName, job.Title, companyName, job.Id); }
                    catch { /* email xətası bloklamamalıdır */ }
                }
            }
        }

        public async Task<PagedResponse<JobListDto>> GetJobsAsync(JobFilterDto filter, int? currentUserId)
        {
            // PageSize limiti
            if (filter.PageSize > 50) filter.PageSize = 50;

            var query = _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .AsQueryable();

            // Filterlər
            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(j => j.Status == filter.Status);
            else
                query = query.Where(j => j.Status == "active");

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.ToLower();
                query = query.Where(j =>
                    j.Title.ToLower().Contains(kw) ||
                    j.Description.ToLower().Contains(kw) ||
                    j.Company.Name.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(j => j.Location.ToLower().Contains(filter.Location.ToLower()));

            if (filter.CategoryId.HasValue)
                query = query.Where(j => j.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filter.JobType))
            {
                var types = filter.JobType.Split(',');
                query = query.Where(j => types.Contains(j.JobType));
            }

            if (!string.IsNullOrWhiteSpace(filter.ExperienceLevel))
                query = query.Where(j => j.ExperienceLevel == filter.ExperienceLevel);

            if (filter.SalaryMin.HasValue)
                query = query.Where(j => j.SalaryMax >= filter.SalaryMin.Value);

            if (filter.SalaryMax.HasValue)
                query = query.Where(j => j.SalaryMin <= filter.SalaryMax.Value);

            if (filter.IsRemote.HasValue)
                query = query.Where(j => j.IsRemote == filter.IsRemote.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(j => j.IsFeatured == filter.IsFeatured.Value);

            if (filter.IsUrgent.HasValue)
                query = query.Where(j => j.IsUrgent == filter.IsUrgent.Value);

            if (filter.CompanyId.HasValue)
                query = query.Where(j => j.CompanyId == filter.CompanyId.Value);

            // Sıralama
            query = filter.SortBy switch
            {
                "oldest" => query.OrderBy(j => j.CreatedAt),
                "salary_asc" => query.OrderBy(j => j.SalaryMin),
                "salary_desc" => query.OrderByDescending(j => j.SalaryMax),
                _ => query.OrderByDescending(j => j.IsFeatured)
                                     .ThenByDescending(j => j.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(j => MapToListDto(j))
                .ToListAsync();

            return new PagedResponse<JobListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<JobDetailDto> GetJobByIdAsync(int id, int? currentUserId, string? ipAddress)
        {
            var job = await _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Include(j => j.RequiredSkills)
                .FirstOrDefaultAsync(j => j.Id == id)
                ?? throw new NotFoundException("İlan tapılmadı.");

            await IncrementViewCountAsync(job, ipAddress);

            return await MapToDetailDtoAsync(job, currentUserId);
        }

        public async Task<JobDetailDto> GetJobBySlugAsync(string slug, int? currentUserId, string? ipAddress)
        {
            var job = await _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Include(j => j.RequiredSkills)
                .FirstOrDefaultAsync(j => j.Slug == slug)
                ?? throw new NotFoundException("İlan tapılmadı.");

            await IncrementViewCountAsync(job, ipAddress);

            return await MapToDetailDtoAsync(job, currentUserId);
        }

        public async Task<IEnumerable<JobListDto>> GetFeaturedJobsAsync()
        {
            return await _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.IsFeatured && j.Status == "active" && j.Deadline >= DateTime.UtcNow)
                .OrderByDescending(j => j.CreatedAt)
                .Take(8)
                .Select(j => MapToListDto(j))
                .ToListAsync();
        }

        public async Task<IEnumerable<JobListDto>> GetRecentJobsAsync(int count)
        {
            if (count > 20) count = 20;
            return await _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.Status == "active")
                .OrderByDescending(j => j.CreatedAt)
                .Take(count)
                .Select(j => MapToListDto(j))
                .ToListAsync();
        }

        public async Task<IEnumerable<JobListDto>> GetRelatedJobsAsync(int jobId)
        {
            var job = await _db.Jobs.FindAsync(jobId)
                ?? throw new NotFoundException("İlan tapılmadı.");

            return await _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.Id != jobId &&
                            j.Status == "active" &&
                            j.CategoryId == job.CategoryId)
                .OrderByDescending(j => j.CreatedAt)
                .Take(6)
                .Select(j => MapToListDto(j))
                .ToListAsync();
        }

        public async Task<JobDetailDto> CreateJobAsync(JobCreateDto dto, int userId)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət profili tapılmadı.");

            // 1) Profil tam doldurulmalıdır
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(company.Description)) missing.Add("Təsvir (Description)");
            if (string.IsNullOrWhiteSpace(company.Industry)) missing.Add("Sənaye (Industry)");
            if (string.IsNullOrWhiteSpace(company.Location)) missing.Add("Məkan (Location)");
            if (string.IsNullOrWhiteSpace(company.Phone)) missing.Add("Telefon (Phone)");
            if (string.IsNullOrWhiteSpace(company.Email)) missing.Add("Email");
            if (missing.Any())
                throw new BadRequestException(
                    "İş elanı yerləşdirmədən əvvəl şirkət profilini tam doldurun. Çatışmayan: " +
                    string.Join(", ", missing) + ".");

            // 2) Şirkət admin tərəfindən təsdiqlənməlidir
            if (!company.IsVerified)
                throw new BadRequestException(
                    "Şirkətiniz hələ admin tərəfindən təsdiqlənməyib. Təsdiqdən sonra iş elanı yerləşdirə biləcəksiniz.");

            var category = await _db.Categories.FindAsync(dto.CategoryId)
                ?? throw new NotFoundException("Kateqoriya tapılmadı.");

            var slug = await SlugHelper.GenerateUniqueJobSlugAsync(dto.Title, _db);

            var job = new Job
            {
                CompanyId = company.Id,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Slug = slug,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Responsibilities = dto.Responsibilities,
                Benefits = dto.Benefits,
                Location = dto.Location,
                IsRemote = dto.IsRemote,
                JobType = dto.JobType,
                ExperienceLevel = dto.ExperienceLevel,
                SalaryMin = dto.SalaryMin,
                SalaryMax = dto.SalaryMax,
                SalaryCurrency = dto.SalaryCurrency ?? "USD",
                SalaryPeriod = dto.SalaryPeriod,
                IsSalaryVisible = dto.IsSalaryVisible,
                IsUrgent = dto.IsUrgent,
                Status = "active",
                Deadline = dto.Deadline,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Jobs.Add(job);
            await _db.SaveChangesAsync();

            // Skills
            if (dto.RequiredSkills.Any())
            {
                var skills = dto.RequiredSkills.Select(s => new JobSkill
                {
                    JobId = job.Id,
                    Name = s
                });
                _db.JobSkills.AddRange(skills);
                await _db.SaveChangesAsync();
            }

            await InvalidateJobCacheAsync();

            // Uyğun job alert-i olan istifadəçilərə bildiriş + email
            try { await NotifyMatchingAlertsAsync(job, company.Name, dto.RequiredSkills?.ToList() ?? new List<string>()); } catch { /* bloklamamalıdır */ }

            return await GetJobByIdAsync(job.Id, userId, null);
        }

        public async Task<JobDetailDto> UpdateJobAsync(int id, JobUpdateDto dto, int userId, string userRole)
        {
            var job = await _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.RequiredSkills)
                .FirstOrDefaultAsync(j => j.Id == id)
                ?? throw new NotFoundException("İlan tapılmadı.");

            if (userRole != "admin" && job.Company.UserId != userId)
                throw new ForbiddenException("Bu ilanı yeniləmək icazəniz yoxdur.");

            job.Title = dto.Title;
            job.Slug = await SlugHelper.GenerateUniqueJobSlugAsync(dto.Title, _db, id);
            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.Responsibilities = dto.Responsibilities;
            job.Benefits = dto.Benefits;
            job.Location = dto.Location;
            job.IsRemote = dto.IsRemote;
            job.JobType = dto.JobType;
            job.ExperienceLevel = dto.ExperienceLevel;
            job.CategoryId = dto.CategoryId;
            job.SalaryMin = dto.SalaryMin;
            job.SalaryMax = dto.SalaryMax;
            job.SalaryCurrency = dto.SalaryCurrency;
            job.SalaryPeriod = dto.SalaryPeriod;
            job.IsSalaryVisible = dto.IsSalaryVisible;
            job.IsUrgent = dto.IsUrgent;
            job.Deadline = dto.Deadline;
            job.UpdatedAt = DateTime.UtcNow;

            // Skills yenilə
            _db.JobSkills.RemoveRange(job.RequiredSkills);
            if (dto.RequiredSkills.Any())
            {
                _db.JobSkills.AddRange(dto.RequiredSkills.Select(s => new JobSkill
                {
                    JobId = job.Id,
                    Name = s
                }));
            }

            await _db.SaveChangesAsync();
            await InvalidateJobCacheAsync();

            return await GetJobByIdAsync(job.Id, userId, null);
        }

        public async Task DeleteJobAsync(int id, int userId, string userRole)
        {
            var job = await _db.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id)
                ?? throw new NotFoundException("İlan tapılmadı.");

            if (userRole != "admin" && job.Company.UserId != userId)
                throw new ForbiddenException("Bu ilanı silmək icazəniz yoxdur.");

            job.IsDeleted = true;
            job.Status = "closed";
            job.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await InvalidateJobCacheAsync();
        }

        public async Task UpdateJobStatusAsync(int id, string status, int userId, string userRole)
        {
            var validStatuses = new[] { "active", "closed", "draft" };
            if (!validStatuses.Contains(status))
                throw new BadRequestException("Yanlış status dəyəri.");

            var job = await _db.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id)
                ?? throw new NotFoundException("İlan tapılmadı.");

            if (userRole != "admin" && job.Company.UserId != userId)
                throw new ForbiddenException("İcazəniz yoxdur.");

            job.Status = status;
            job.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task UpdateJobFeaturedAsync(int id, bool isFeatured)
        {
            var job = await _db.Jobs.FindAsync(id)
                ?? throw new NotFoundException("İlan tapılmadı.");

            job.IsFeatured = isFeatured;
            job.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await InvalidateJobCacheAsync();
        }

        public async Task<PagedResponse<JobListDto>> GetMyJobsAsync(int userId, string? status, int page, int pageSize)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət profili tapılmadı.");

            var query = _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .IgnoreQueryFilters()
                .Where(j => j.CompanyId == company.Id && !j.IsDeleted);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(j => j.Status == status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => MapToListDto(j))
                .ToListAsync();

            return new PagedResponse<JobListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // --- Private helpers ---

        private async Task IncrementViewCountAsync(Job job, string? ipAddress)
        {
            if (_redis != null && !string.IsNullOrWhiteSpace(ipAddress))
            {
                var db = _redis.GetDatabase();
                var key = $"job:views:{job.Id}:{ipAddress}";
                var exists = await db.KeyExistsAsync(key);
                if (exists) return;
                await db.StringSetAsync(key, "1", TimeSpan.FromHours(24));
            }

            job.ViewCount++;
            await _db.SaveChangesAsync();
        }

        private async Task<JobDetailDto> MapToDetailDtoAsync(Job job, int? currentUserId)
        {
            var isSaved = false;
            var hasApplied = false;

            if (currentUserId.HasValue)
            {
                isSaved = await _db.SavedJobs
                    .AnyAsync(s => s.UserId == currentUserId.Value && s.JobId == job.Id);
                hasApplied = await _db.JobApplications
                    .AnyAsync(a => a.UserId == currentUserId.Value && a.JobId == job.Id);
            }

            var listDto = MapToListDto(job);

            return new JobDetailDto
            {
                Id = listDto.Id,
                Title = listDto.Title,
                Slug = listDto.Slug,
                Location = listDto.Location,
                IsRemote = listDto.IsRemote,
                JobType = listDto.JobType,
                ExperienceLevel = listDto.ExperienceLevel,
                SalaryMin = listDto.SalaryMin,
                SalaryMax = listDto.SalaryMax,
                SalaryCurrency = listDto.SalaryCurrency,
                IsSalaryVisible = listDto.IsSalaryVisible,
                IsFeatured = listDto.IsFeatured,
                IsUrgent = listDto.IsUrgent,
                Deadline = listDto.Deadline,
                CreatedAt = listDto.CreatedAt,
                ViewCount = listDto.ViewCount,
                ApplicationCount = listDto.ApplicationCount,
                Company = listDto.Company,
                Category = listDto.Category,
                Description = job.Description,
                Requirements = job.Requirements,
                Responsibilities = job.Responsibilities,
                Benefits = job.Benefits,
                SalaryPeriod = job.SalaryPeriod,
                Status = job.Status,
                RequiredSkills = job.RequiredSkills.Select(s => s.Name).ToList(),
                IsSaved = isSaved,
                HasApplied = hasApplied
            };
        }

        private static JobListDto MapToListDto(Job j) => new()
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
            SalaryCurrency = j.SalaryCurrency,
            IsSalaryVisible = j.IsSalaryVisible,
            IsFeatured = j.IsFeatured,
            IsUrgent = j.IsUrgent,
            Deadline = j.Deadline,
            CreatedAt = j.CreatedAt,
            ViewCount = j.ViewCount,
            ApplicationCount = j.Applications?.Count ?? 0,
            Status = j.Status,
            Company = new JobCompanyDto
            {
                Id = j.Company.Id,
                Name = j.Company.Name,
                LogoUrl = j.Company.LogoUrl,
                IsVerified = j.Company.IsVerified,
                Location = j.Company.Location
            },
            Category = new JobCategoryDto
            {
                Id = j.Category.Id,
                Name = j.Category.Name,
                IconClass = j.Category.IconClass
            }
        };

        private async Task InvalidateJobCacheAsync()
        {
            if (_redis == null) return;
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync("jobs:featured");
            await db.KeyDeleteAsync("jobs:recent");
        }
    }
}
