using JobBoard.Core.DTOs.Alerts;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api")]
    [Authorize]
    [Produces("application/json")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;
        private readonly ISavedJobService _savedJobService;
        private readonly INotificationService _notificationService;

        public AlertsController(
            IAlertService alertService,
            ISavedJobService savedJobService,
            INotificationService notificationService)
        {
            _alertService = alertService;
            _savedJobService = savedJobService;
            _notificationService = notificationService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // --- ALERTS ---

        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            var result = await _alertService.GetAlertsAsync(UserId);
            return Ok(ApiResponse<IEnumerable<AlertDto>>.Ok(result));
        }

        [HttpPost("alerts")]
        public async Task<IActionResult> CreateAlert([FromBody] AlertCreateDto dto)
        {
            var result = await _alertService.CreateAlertAsync(UserId, dto);
            return StatusCode(201, ApiResponse<AlertDto>.Ok(result));
        }

        [HttpPut("alerts/{id:int}")]
        public async Task<IActionResult> UpdateAlert(int id, [FromBody] AlertUpdateDto dto)
        {
            var result = await _alertService.UpdateAlertAsync(UserId, id, dto);
            return Ok(ApiResponse<AlertDto>.Ok(result));
        }

        [HttpDelete("alerts/{id:int}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            await _alertService.DeleteAlertAsync(UserId, id);
            return Ok(ApiResponse.Ok("Alert silindi."));
        }

        [HttpPatch("alerts/{id:int}/toggle")]
        public async Task<IActionResult> ToggleAlert(int id, [FromBody] AlertToggleDto dto)
        {
            await _alertService.ToggleAlertAsync(UserId, id, dto.IsActive);
            return Ok(ApiResponse.Ok("Alert yeniləndi."));
        }

        // --- SAVED JOBS ---

        [HttpGet("saved-jobs")]
        public async Task<IActionResult> GetSavedJobs(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var result = await _savedJobService.GetSavedJobsAsync(UserId, page, pageSize);
            return Ok(ApiResponse<PagedResponse<SavedJobDto>>.Ok(result));
        }

        [HttpPost("saved-jobs/{jobId:int}")]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            await _savedJobService.SaveJobAsync(UserId, jobId);
            return StatusCode(201, ApiResponse.Ok("İlan saxlanıldı."));
        }

        [HttpDelete("saved-jobs/{jobId:int}")]
        public async Task<IActionResult> UnsaveJob(int jobId)
        {
            await _savedJobService.UnsaveJobAsync(UserId, jobId);
            return Ok(ApiResponse.Ok("İlan siyahıdan çıxarıldı."));
        }

        // --- NOTIFICATIONS ---

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var result = await _notificationService.GetNotificationsAsync(UserId);
            return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(result));
        }

        [HttpPatch("notifications/{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(UserId, id);
            return Ok(ApiResponse.Ok("Bildiriş oxundu."));
        }

        [HttpPatch("notifications/read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(UserId);
            return Ok(ApiResponse.Ok("Bütün bildirişlər oxundu."));
        }
    }
}
