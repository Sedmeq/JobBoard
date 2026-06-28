using JobBoard.Core.DTOs.Applications;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/applications")]
    [Authorize]
    [Produces("application/json")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _appService;
        public ApplicationsController(IApplicationService appService) => _appService = appService;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string UserRole => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpPost]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> Create([FromBody] ApplicationCreateDto dto)
        {
            var result = await _appService.CreateAsync(dto, UserId);
            return StatusCode(201, ApiResponse<ApplicationListDto>.Ok(result, "Müraciətiniz göndərildi."));
        }

        [HttpGet("my")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> GetMy([FromQuery] ApplicationFilterDto filter)
        {
            var result = await _appService.GetMyApplicationsAsync(UserId, filter);
            return Ok(ApiResponse<PagedResponse<ApplicationListDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _appService.GetByIdAsync(id, UserId, UserRole);
            return Ok(ApiResponse<ApplicationListDto>.Ok(result));
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ApplicationStatusUpdateDto dto)
        {
            await _appService.UpdateStatusAsync(id, dto, UserId, UserRole);
            return Ok(ApiResponse.Ok("Status yeniləndi."));
        }

        [HttpPatch("{id:int}/withdraw")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> Withdraw(int id)
        {
            await _appService.WithdrawAsync(id, UserId);
            return Ok(ApiResponse.Ok("Müraciət geri çəkildi."));
        }

        [HttpGet("stats")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _appService.GetStatsAsync(UserId);
            return Ok(ApiResponse<ApplicationStatsDto>.Ok(result));
        }

        [HttpGet("applicants")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> GetCompanyApplicants([FromQuery] int page = 1, [FromQuery] int pageSize = 8)
        {
            var result = await _appService.GetCompanyApplicantsAsync(UserId, page, pageSize);
            return Ok(ApiResponse<PagedResponse<CompanyApplicantDto>>.Ok(result));
        }
    }
}
