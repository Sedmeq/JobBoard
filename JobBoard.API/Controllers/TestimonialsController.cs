using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Testimonials;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.API.Controllers
{
    [ApiController]
    [Route("api/testimonials")]
    [Produces("application/json")]
    public class TestimonialsController : ControllerBase
    {
        private readonly ITestimonialService _service;
        public TestimonialsController(ITestimonialService service) => _service = service;

        // Public — ana səhifə üçün aktiv rəylər
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            var result = await _service.GetAllAsync(onlyActive: true);
            return Ok(ApiResponse<IEnumerable<TestimonialDto>>.Ok(result));
        }

        // Admin — hamısı
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(onlyActive: false);
            return Ok(ApiResponse<IEnumerable<TestimonialDto>>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromForm] TestimonialCreateDto dto, IFormFile? avatar)
        {
            var result = await _service.CreateAsync(dto, avatar);
            return StatusCode(201, ApiResponse<TestimonialDto>.Ok(result, "Rəy əlavə edildi."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromForm] TestimonialCreateDto dto, IFormFile? avatar)
        {
            var result = await _service.UpdateAsync(id, dto, avatar);
            return Ok(ApiResponse<TestimonialDto>.Ok(result, "Rəy yeniləndi."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Rəy silindi."));
        }
    }
}
