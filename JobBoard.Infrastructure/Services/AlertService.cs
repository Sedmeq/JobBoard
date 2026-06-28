using JobBoard.Core.DTOs.Alerts;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Jobs;
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

    public class AlertService : IAlertService
    {
        private readonly AppDbContext _db;
        public AlertService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<AlertDto>> GetAlertsAsync(int userId)
            => await _db.JobAlerts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => MapToDto(a))
                .ToListAsync();

        public async Task<AlertDto> CreateAlertAsync(int userId, AlertCreateDto dto)
        {
            var alert = new JobAlert
            {
                UserId = userId,
                Name = dto.Name,
                Keyword = dto.Keyword,
                Location = dto.Location,
                CategoryId = dto.CategoryId,
                JobType = dto.JobType,
                Frequency = dto.Frequency,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.JobAlerts.Add(alert);
            await _db.SaveChangesAsync();
            return MapToDto(alert);
        }

        public async Task<AlertDto> UpdateAlertAsync(int userId, int alertId, AlertUpdateDto dto)
        {
            var alert = await _db.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId)
                ?? throw new NotFoundException("Alert tapılmadı.");

            alert.Name = dto.Name;
            alert.Keyword = dto.Keyword;
            alert.Location = dto.Location;
            alert.CategoryId = dto.CategoryId;
            alert.JobType = dto.JobType;
            alert.Frequency = dto.Frequency;

            await _db.SaveChangesAsync();
            return MapToDto(alert);
        }

        public async Task DeleteAlertAsync(int userId, int alertId)
        {
            var alert = await _db.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId)
                ?? throw new NotFoundException("Alert tapılmadı.");

            _db.JobAlerts.Remove(alert);
            await _db.SaveChangesAsync();
        }

        public async Task ToggleAlertAsync(int userId, int alertId, bool isActive)
        {
            var alert = await _db.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId)
                ?? throw new NotFoundException("Alert tapılmadı.");

            alert.IsActive = isActive;
            await _db.SaveChangesAsync();
        }

        private static AlertDto MapToDto(JobAlert a) => new()
        {
            Id = a.Id,
            Name = a.Name,
            Keyword = a.Keyword,
            Location = a.Location,
            CategoryId = a.CategoryId,
            JobType = a.JobType,
            Frequency = a.Frequency,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt
        };
    }

    public class SavedJobService : ISavedJobService
    {
        private readonly AppDbContext _db;
        public SavedJobService(AppDbContext db) => _db = db;

        public async Task<PagedResponse<SavedJobDto>> GetSavedJobsAsync(int userId, int page, int pageSize)
        {
            var query = _db.SavedJobs
                .Include(s => s.Job)
                    .ThenInclude(j => j.Company)
                .Include(s => s.Job)
                    .ThenInclude(j => j.Category)
                .Include(s => s.Job)
                    .ThenInclude(j => j.Applications)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(s => new SavedJobDto
            {
                Id = s.Id,
                JobId = s.JobId,
                SavedAt = s.SavedAt,
                Job = new JobListDto
                {
                    Id = s.Job.Id,
                    Title = s.Job.Title,
                    Slug = s.Job.Slug,
                    Location = s.Job.Location,
                    IsRemote = s.Job.IsRemote,
                    JobType = s.Job.JobType,
                    ExperienceLevel = s.Job.ExperienceLevel,
                    SalaryMin = s.Job.SalaryMin,
                    SalaryMax = s.Job.SalaryMax,
                    SalaryCurrency = s.Job.SalaryCurrency,
                    IsSalaryVisible = s.Job.IsSalaryVisible,
                    IsFeatured = s.Job.IsFeatured,
                    IsUrgent = s.Job.IsUrgent,
                    Deadline = s.Job.Deadline,
                    CreatedAt = s.Job.CreatedAt,
                    ViewCount = s.Job.ViewCount,
                    ApplicationCount = s.Job.Applications.Count,
                    Company = new JobCompanyDto
                    {
                        Id = s.Job.Company.Id,
                        Name = s.Job.Company.Name,
                        LogoUrl = s.Job.Company.LogoUrl,
                        IsVerified = s.Job.Company.IsVerified,
                        Location = s.Job.Company.Location
                    },
                    Category = new JobCategoryDto
                    {
                        Id = s.Job.Category.Id,
                        Name = s.Job.Category.Name,
                        IconClass = s.Job.Category.IconClass
                    }
                }
            }).ToList();

            return new PagedResponse<SavedJobDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task SaveJobAsync(int userId, int jobId)
        {
            var job = await _db.Jobs.FindAsync(jobId)
                ?? throw new NotFoundException("İlan tapılmadı.");

            var exists = await _db.SavedJobs
                .AnyAsync(s => s.UserId == userId && s.JobId == jobId);

            if (exists)
                throw new ConflictException("Bu ilan artıq saxlanılıb.");

            _db.SavedJobs.Add(new SavedJob
            {
                UserId = userId,
                JobId = jobId,
                SavedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        public async Task UnsaveJobAsync(int userId, int jobId)
        {
            var saved = await _db.SavedJobs
                .FirstOrDefaultAsync(s => s.UserId == userId && s.JobId == jobId)
                ?? throw new NotFoundException("Saxlanmış ilan tapılmadı.");

            _db.SavedJobs.Remove(saved);
            await _db.SaveChangesAsync();
        }
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly INotificationPublisher _publisher;
        public NotificationService(AppDbContext db, INotificationPublisher publisher)
        {
            _db = db;
            _publisher = publisher;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId)
            => await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    ActionUrl = n.ActionUrl,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

        public async Task MarkAsReadAsync(int userId, int notificationId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
                ?? throw new NotFoundException("Bildiriş tapılmadı.");

            notification.IsRead = true;
            await _db.SaveChangesAsync();
            await PushUnreadCountAsync(userId);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _db.SaveChangesAsync();
            await PushUnreadCountAsync(userId);
        }

        public async Task CreateNotificationAsync(
            int userId, string title, string message, string type, string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            // Real-time yayım (SignalR)
            await _publisher.PushToUserAsync(userId, new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = false,
                ActionUrl = notification.ActionUrl,
                CreatedAt = notification.CreatedAt
            });
            await PushUnreadCountAsync(userId);
        }

        private async Task PushUnreadCountAsync(int userId)
        {
            var unread = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            await _publisher.PushUnreadCountAsync(userId, unread);
        }
    }
}
