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
                .Include(u => u.CandidateProfile)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            string? resumeUrl = dto.ResumeUrl;
            if (dto.UseProfileResume && string.IsNullOrWhiteSpace(resumeUrl))
                resumeUrl = user.CandidateProfile?.ResumeUrl;

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

            // İşəgötürənə email
            if (job.Company?.User != null)
            {
                await _emailService.SendNewApplicationAsync(
                    job.Company.User.Email,
                    job.Company.User.FullName,
                    user.FullName,
                    job.Title);
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
