using JobBoard.Core.DTOs.Categories;
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
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

        private int? CurrentUserId =>
            User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeJobCount = true)
        {
            var result = await _categoryService.GetAllAsync(includeJobCount);
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(result));
        }

        [HttpGet("{slug}/jobs")]
        public async Task<IActionResult> GetJobsByCategory(
            string slug,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _categoryService.GetJobsByCategoryAsync(slug, page, pageSize, CurrentUserId);
            return Ok(ApiResponse<PagedResponse<JobListDto>>.Ok(result));
        }

        [HttpPost]
        [HttpPost("~/api/admin/categories")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var result = await _categoryService.CreateAsync(dto);
            return StatusCode(201, ApiResponse<CategoryDto>.Ok(result, "Kateqoriya yaradıldı."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CategoryDto>.Ok(result, "Kateqoriya yeniləndi."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Kateqoriya silindi."));
        }
    }
}
