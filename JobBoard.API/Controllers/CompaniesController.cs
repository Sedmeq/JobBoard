using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Companies;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/companies")]
    [Produces("application/json")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        public CompaniesController(ICompanyService companyService) => _companyService = companyService;

        private int? CurrentUserId =>
            User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

        [HttpGet]
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyFilterDto filter)
        {
            var result = await _companyService.GetCompaniesAsync(filter);
            return Ok(ApiResponse<PagedResponse<CompanyListDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _companyService.GetCompanyByIdAsync(id, CurrentUserId);
            return Ok(ApiResponse<CompanyDetailDto>.Ok(result));
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured()
        {
            var result = await _companyService.GetFeaturedCompaniesAsync();
            return Ok(ApiResponse<IEnumerable<CompanyListDto>>.Ok(result));
        }

        [HttpGet("me")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> GetMyCompany()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _companyService.GetMyCompanyAsync(userId);
            return Ok(ApiResponse<CompanyDetailDto>.Ok(result));
        }

        [HttpPut("me")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> UpdateMyCompany([FromBody] CompanyUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _companyService.UpdateMyCompanyAsync(userId, dto);
            return Ok(ApiResponse<CompanyDetailDto>.Ok(result, "Şirkət profili yeniləndi."));
        }

        [HttpPost("me/logo")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var url = await _companyService.UploadLogoAsync(userId, file);
            return Ok(ApiResponse<object>.Ok(new { url }, "Logo yükləndi."));
        }

        [HttpPost("me/cover")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> UploadCover(IFormFile file)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var url = await _companyService.UploadCoverAsync(userId, file);
            return Ok(ApiResponse<object>.Ok(new { url }, "Cover şəkli yükləndi."));
        }

        [HttpGet("{id:int}/reviews")]
        public async Task<IActionResult> GetReviews(int id,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _companyService.GetReviewsAsync(id, page, pageSize);
            return Ok(ApiResponse<PagedResponse<CompanyReviewDto>>.Ok(result));
        }

        [HttpPost("{id:int}/reviews")]
        [Authorize(Roles = "candidate")]
        public async Task<IActionResult> AddReview(int id, [FromBody] CompanyReviewCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _companyService.AddReviewAsync(id, userId, dto);
            return StatusCode(201, ApiResponse.Ok("Rəy əlavə edildi."));
        }
    }
}
