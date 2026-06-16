using JobBoard.Core.DTOs.Alerts;
using JobBoard.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{

    public interface IAlertService
    {
        Task<IEnumerable<AlertDto>> GetAlertsAsync(int userId);
        Task<AlertDto> CreateAlertAsync(int userId, AlertCreateDto dto);
        Task<AlertDto> UpdateAlertAsync(int userId, int alertId, AlertUpdateDto dto);
        Task DeleteAlertAsync(int userId, int alertId);
        Task ToggleAlertAsync(int userId, int alertId, bool isActive);
    }

    public interface ISavedJobService
    {
        Task<PagedResponse<SavedJobDto>> GetSavedJobsAsync(int userId, int page, int pageSize);
        Task SaveJobAsync(int userId, int jobId);
        Task UnsaveJobAsync(int userId, int jobId);
    }

    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId);
        Task MarkAsReadAsync(int userId, int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task CreateNotificationAsync(int userId, string title, string message, string type, string? actionUrl = null);
    }
}
