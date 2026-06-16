using JobBoard.Core.DTOs.Applications;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Jobs;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/jobs")]
    [Produces("application/json")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService) => _jobService = jobService;

        private int? CurrentUserId =>
            User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

        private string? UserRole =>
            User.FindFirstValue(ClaimTypes.Role);

        private string? ClientIp =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] JobFilterDto filter)
        {
            var result = await _jobService.GetJobsAsync(filter, CurrentUserId);
            return Ok(ApiResponse<PagedResponse<JobListDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _jobService.GetJobByIdAsync(id, CurrentUserId, ClientIp);
            return Ok(ApiResponse<JobDetailDto>.Ok(result));
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _jobService.GetJobBySlugAsync(slug, CurrentUserId, ClientIp);
            return Ok(ApiResponse<JobDetailDto>.Ok(result));
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured()
        {
            var result = await _jobService.GetFeaturedJobsAsync();
            return Ok(ApiResponse<IEnumerable<JobListDto>>.Ok(result));
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 6)
        {
            var result = await _jobService.GetRecentJobsAsync(count);
            return Ok(ApiResponse<IEnumerable<JobListDto>>.Ok(result));
        }

        [HttpGet("related/{jobId:int}")]
        public async Task<IActionResult> GetRelated(int jobId)
        {
            var result = await _jobService.GetRelatedJobsAsync(jobId);
            return Ok(ApiResponse<IEnumerable<JobListDto>>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> Create([FromBody] JobCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _jobService.CreateJobAsync(dto, userId);
            return StatusCode(201, ApiResponse<JobDetailDto>.Ok(result, "İlan uğurla yaradıldı."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] JobUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _jobService.UpdateJobAsync(id, dto, userId, UserRole!);
            return Ok(ApiResponse<JobDetailDto>.Ok(result, "İlan yeniləndi."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _jobService.DeleteJobAsync(id, userId, UserRole!);
            return Ok(ApiResponse.Ok("İlan silindi."));
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] JobStatusDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _jobService.UpdateJobStatusAsync(id, dto.Status, userId, UserRole!);
            return Ok(ApiResponse.Ok("Status yeniləndi."));
        }

        [HttpPatch("{id:int}/featured")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateFeatured(int id, [FromBody] JobFeaturedDto dto)
        {
            await _jobService.UpdateJobFeaturedAsync(id, dto.IsFeatured);
            return Ok(ApiResponse.Ok("Featured status yeniləndi."));
        }

        [HttpGet("my-jobs")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> GetMyJobs(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _jobService.GetMyJobsAsync(userId, status, page, pageSize);
            return Ok(ApiResponse<PagedResponse<JobListDto>>.Ok(result));
        }

        [HttpGet("{id:int}/applicants")]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> GetApplicants(
    int id, [FromQuery] ApplicationFilterDto filter,
    [FromServices] IApplicationService applicationService)
        {
            var result = await applicationService.GetJobApplicantsAsync(
                id, CurrentUserId ?? 0, UserRole ?? "", filter);
            return Ok(ApiResponse<PagedResponse<ApplicationListDto>>.Ok(result));
        }
    }
}
