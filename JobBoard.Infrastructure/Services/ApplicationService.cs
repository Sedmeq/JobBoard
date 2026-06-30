using JobBoard.Core.DTOs.Applications;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public ApplicationService(
            AppDbContext db,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _db = db;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<ApplicationListDto> CreateAsync(ApplicationCreateDto dto, int userId)
        {
            var job = await _db.Jobs
                .Include(j => j.Company)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(j => j.Id == dto.JobId)
                ?? throw new NotFoundException("İlan tapılmadı.");

            if (job.Status != "active")
                throw new BadRequestException("Bu ilana müraciət etmək mümkün deyil.");

            if (job.Deadline < DateTime.UtcNow)
                throw new BadRequestException("Bu ilanın müraciət müddəti bitib.");

            var alreadyApplied = await _db.JobApplications
                .AnyAsync(a => a.UserId == userId && a.JobId == dto.JobId);

            if (alreadyApplied)
                throw new ConflictException("Bu ilana artıq müraciət etmisiniz.");

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            // CandidateProfile navigation-u DbContext-də WithOne() boş konfiqurasiya
            // edildiyi üçün Include ilə düzgün yüklənmir. Ona görə profili birbaşa
            // UserId üzrə sorğulayırıq (My Resume səhifəsi ilə eyni üsul).
            var profile = await _db.CandidateProfiles
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Profil tamamlanmayıbsa müraciətə icazə verilmir
            var missing = new List<string>();
            if (profile == null || string.IsNullOrWhiteSpace(profile.Headline)) missing.Add("Başlıq (Headline)");
            if (profile == null || string.IsNullOrWhiteSpace(profile.Summary)) missing.Add("Xülasə (Summary)");
            if (profile == null || string.IsNullOrWhiteSpace(profile.Location)) missing.Add("Məkan (Location)");
            if (profile == null || profile.Skills == null || !profile.Skills.Any()) missing.Add("Bacarıqlar (Skills)");

            if (missing.Any())
                throw new BadRequestException(
                    "Müraciət etməzdən əvvəl profilinizi tamamlayın. Çatışmayan məlumatlar: " +
                    string.Join(", ", missing) + ".");

            string? resumeUrl = dto.ResumeUrl;
            if (dto.UseProfileResume && string.IsNullOrWhiteSpace(resumeUrl))
                resumeUrl = profile?.ResumeUrl;

            var application = new JobApplication
            {
                JobId = dto.JobId,
                UserId = userId,
                CoverLetter = dto.CoverLetter,
                ResumeUrl = resumeUrl,
                Status = "pending",
                AppliedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.JobApplications.Add(application);
            await _db.SaveChangesAsync();

            // Namizədə email + bildiriş
            await _emailService.SendApplicationReceivedAsync(
                user.Email, user.FullName, job.Title);

            await _notificationService.CreateNotificationAsync(
                userId,
                "Müraciətiniz qəbul edildi",
                $"{job.Title} vəzifəsinə müraciətiniz uğurla göndərildi.",
                "application_status",
                $"/candidate/applications/{application.Id}");

            // İşəgötürənə yalnız bildiriş (email göndərilmir)
            if (job.Company?.User != null)
            {
                await _notificationService.CreateNotificationAsync(
                    job.Company.User.Id,
                    "Yeni müraciət",
                    $"{user.FullName} \"{job.Title}\" vəzifəsinə müraciət etdi.",
                    "new_application",
                    $"/company/jobs/{job.Id}/applications");
            }

            return await MapToDto(application, includeCandidate: false);
        }

        public async Task<PagedResponse<ApplicationListDto>> GetMyApplicationsAsync(
            int userId, ApplicationFilterDto filter)
        {
            var query = _db.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(a => a.Status == filter.Status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.AppliedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = new List<ApplicationListDto>();
            foreach (var a in items)
                dtos.Add(await MapToDto(a, includeCandidate: false));

            return new PagedResponse<ApplicationListDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ApplicationListDto> GetByIdAsync(int id, int userId, string userRole)
        {
            var application = await _db.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.User)
                    .ThenInclude(u => u.CandidateProfile)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException("Müraciət tapılmadı.");

            if (userRole == "candidate" && application.UserId != userId)
                throw new ForbiddenException("Bu müraciətə baxmaq icazəniz yoxdur.");

            if (userRole == "employer")
            {
                var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
                if (company == null || application.Job.CompanyId != company.Id)
                    throw new ForbiddenException("Bu müraciətə baxmaq icazəniz yoxdur.");
            }

            return await MapToDto(application, includeCandidate: true);
        }

        public async Task UpdateStatusAsync(int id, ApplicationStatusUpdateDto dto, int userId, string userRole)
        {
            var validStatuses = new[] { "reviewing", "shortlisted", "interview", "offered", "rejected" };
            if (!validStatuses.Contains(dto.Status))
                throw new BadRequestException("Yanlış status dəyəri.");

            var application = await _db.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException("Müraciət tapılmadı.");

            if (userRole == "employer")
            {
                var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
                if (company == null || application.Job.CompanyId != company.Id)
                    throw new ForbiddenException("Bu müraciəti yeniləmək icazəniz yoxdur.");
            }

            application.Status = dto.Status;
            application.EmployerNote = dto.EmployerNote;
            application.InterviewDate = dto.InterviewDate;
            application.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Namizədə bildiriş + email
            await _notificationService.CreateNotificationAsync(
                application.UserId,
                "Müraciət statusu dəyişdi",
                $"{application.Job.Title} vəzifəsinə müraciətinizin statusu yeniləndi: {dto.Status}",
                "application_status",
                $"/candidate/applications/{application.Id}");

            await _emailService.SendApplicationStatusChangedAsync(
                application.User.Email,
                application.User.FullName,
                application.Job.Title,
                dto.Status);
        }

        public async Task WithdrawAsync(int id, int userId)
        {
            var application = await _db.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
                ?? throw new NotFoundException("Müraciət tapılmadı.");

            if (application.Status != "pending" && application.Status != "reviewing")
                throw new BadRequestException("Bu mərhələdə müraciəti geri çəkmək mümkün deyil.");

            application.Status = "withdrawn";
            application.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task<ApplicationStatsDto> GetStatsAsync(int userId)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət tapılmadı.");

            var applications = await _db.JobApplications
                .Where(a => a.Job.CompanyId == company.Id)
                .ToListAsync();

            return new ApplicationStatsDto
            {
                Total = applications.Count,
                Pending = applications.Count(a => a.Status == "pending"),
                Reviewing = applications.Count(a => a.Status == "reviewing"),
                Shortlisted = applications.Count(a => a.Status == "shortlisted"),
                Interview = applications.Count(a => a.Status == "interview"),
                Offered = applications.Count(a => a.Status == "offered"),
                Rejected = applications.Count(a => a.Status == "rejected"),
                Withdrawn = applications.Count(a => a.Status == "withdrawn")
            };
        }

        public async Task<PagedResponse<ApplicationListDto>> GetJobApplicantsAsync(
            int jobId, int userId, string userRole, ApplicationFilterDto filter)
        {
            var job = await _db.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == jobId)
                ?? throw new NotFoundException("İlan tapılmadı.");

            if (userRole == "employer")
            {
                var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
                if (company == null || job.CompanyId != company.Id)
                    throw new ForbiddenException("İcazəniz yoxdur.");
            }

            var query = _db.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.User)
                    .ThenInclude(u => u.CandidateProfile)
                .Where(a => a.JobId == jobId);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(a => a.Status == filter.Status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.AppliedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = new List<ApplicationListDto>();
            foreach (var a in items)
                dtos.Add(await MapToDto(a, includeCandidate: true));

            return new PagedResponse<ApplicationListDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PagedResponse<CompanyApplicantDto>> GetCompanyApplicantsAsync(int userId, int page, int pageSize)
        {
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Şirkət tapılmadı.");

            var apps = await _db.JobApplications
                .Include(a => a.User)
                    .ThenInclude(u => u.CandidateProfile)
                        .ThenInclude(p => p.Skills)
                .Where(a => a.Job.CompanyId == company.Id && a.Status != "withdrawn")
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            var distinct = apps
                .GroupBy(a => a.UserId)
                .Select(g =>
                {
                    var latest = g.First(); // AppliedAt desc sıralanıb
                    var u = latest.User;
                    var profile = u.CandidateProfile;
                    return new CompanyApplicantDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        AvatarUrl = u.AvatarUrl,
                        Headline = profile?.Headline,
                        Location = profile?.Location,
                        ExperienceYears = profile?.ExperienceYears,
                        ResumeUrl = g.Select(x => x.ResumeUrl).FirstOrDefault(x => !string.IsNullOrEmpty(x))
                                    ?? profile?.ResumeUrl,
                        Skills = profile?.Skills.Select(s => s.Name).ToList() ?? [],
                        AppliedJobsCount = g.Count(),
                        LastAppliedAt = g.Max(x => x.AppliedAt)
                    };
                })
                .OrderByDescending(d => d.LastAppliedAt)
                .ToList();

            var total = distinct.Count;
            var items = distinct.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResponse<CompanyApplicantDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private Task<ApplicationListDto> MapToDto(JobApplication a, bool includeCandidate)
        {
            var dto = new ApplicationListDto
            {
                Id = a.Id,
                Status = a.Status,
                CoverLetter = a.CoverLetter,
                ResumeUrl = a.ResumeUrl,
                EmployerNote = a.EmployerNote,
                InterviewDate = a.InterviewDate,
                AppliedAt = a.AppliedAt,
                UpdatedAt = a.UpdatedAt,
                Job = new ApplicationJobDto
                {
                    Id = a.Job.Id,
                    Title = a.Job.Title,
                    Slug = a.Job.Slug,
                    Location = a.Job.Location,
                    JobType = a.Job.JobType,
                    CompanyName = a.Job.Company?.Name ?? "",
                    CompanyLogo = a.Job.Company?.LogoUrl
                }
            };

            if (includeCandidate && a.User != null)
            {
                dto.Candidate = new ApplicationCandidateDto
                {
                    Id = a.User.Id,
                    FullName = a.User.FullName,
                    Email = a.User.Email,
                    AvatarUrl = a.User.AvatarUrl,
                    Headline = a.User.CandidateProfile?.Headline,
                    Location = a.User.CandidateProfile?.Location,
                    ExperienceYears = a.User.CandidateProfile?.ExperienceYears,
                    ResumeUrl = a.ResumeUrl ?? a.User.CandidateProfile?.ResumeUrl
                };
            }

            return Task.FromResult(dto);
        }
    }
}
