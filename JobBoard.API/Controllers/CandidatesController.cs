using JobBoard.Core.DTOs.Applications;
using JobBoard.Core.DTOs.Candidates;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/candidates")]
    [Produces("application/json")]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly IApplicationService _applicationService;

        public CandidatesController(ICandidateService candidateService, IApplicationService applicationService)
        {
            _candidateService = candidateService;
            _applicationService = applicationService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string UserRole => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> GetCandidates([FromQuery] CandidateFilterDto filter)
        {
            var result = await _candidateService.GetCandidatesAsync(filter);
            return Ok(ApiResponse<PagedResponse<CandidateListDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _candidateService.GetByIdAsync(id);
            return Ok(ApiResponse<CandidateDetailDto>.Ok(result));
        }

        [HttpGet("me")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> GetMe()
        {
            var result = await _candidateService.GetMeAsync(UserId);
            return Ok(ApiResponse<CandidateDetailDto>.Ok(result));
        }

        [HttpPut("me")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> UpdateMe([FromBody] CandidateUpdateDto dto)
        {
            var result = await _candidateService.UpdateMeAsync(UserId, dto);
            return Ok(ApiResponse<CandidateDetailDto>.Ok(result, "Profil yeniləndi."));
        }

        [HttpPost("me/experience")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> AddExperience([FromBody] WorkExperienceCreateDto dto)
        {
            var result = await _candidateService.AddExperienceAsync(UserId, dto);
            return StatusCode(201, ApiResponse<WorkExperienceDto>.Ok(result));
        }

        [HttpPut("me/experience/{id:int}")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> UpdateExperience(int id, [FromBody] WorkExperienceCreateDto dto)
        {
            var result = await _candidateService.UpdateExperienceAsync(UserId, id, dto);
            return Ok(ApiResponse<WorkExperienceDto>.Ok(result));
        }

        [HttpDelete("me/experience/{id:int}")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            await _candidateService.DeleteExperienceAsync(UserId, id);
            return Ok(ApiResponse.Ok("Təcrübə silindi."));
        }

        [HttpPost("me/education")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> AddEducation([FromBody] EducationCreateDto dto)
        {
            var result = await _candidateService.AddEducationAsync(UserId, dto);
            return StatusCode(201, ApiResponse<EducationDto>.Ok(result));
        }

        [HttpPut("me/education/{id:int}")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> UpdateEducation(int id, [FromBody] EducationCreateDto dto)
        {
            var result = await _candidateService.UpdateEducationAsync(UserId, id, dto);
            return Ok(ApiResponse<EducationDto>.Ok(result));
        }

        [HttpDelete("me/education/{id:int}")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            await _candidateService.DeleteEducationAsync(UserId, id);
            return Ok(ApiResponse.Ok("Təhsil silindi."));
        }

        [HttpPost("me/resume")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            var url = await _candidateService.UploadResumeAsync(UserId, file);
            return Ok(ApiResponse<object>.Ok(new { url }, "CV yükləndi."));
        }

        [HttpDelete("me/resume")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> DeleteResume()
        {
            await _candidateService.DeleteResumeAsync(UserId);
            return Ok(ApiResponse.Ok("CV silindi."));
        }

        [HttpPost("me/avatar")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var url = await _candidateService.UploadAvatarAsync(UserId, file);
            return Ok(ApiResponse<object>.Ok(new { url }, "Profil şəkli yükləndi."));
        }

        [HttpGet("{id:int}/applications")]
        [Authorize(Roles = "employer,admin")]
        public async Task<IActionResult> GetApplicants(int id, [FromQuery] ApplicationFilterDto filter)
        {
            var result = await _applicationService.GetJobApplicantsAsync(id, UserId, UserRole, filter);
            return Ok(ApiResponse<PagedResponse<ApplicationListDto>>.Ok(result));
        }
    }
}
