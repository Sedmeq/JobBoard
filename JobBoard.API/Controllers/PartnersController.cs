using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Partners;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.API.Controllers
{
    [ApiController]
    [Route("api/partners")]
    [Produces("application/json")]
    public class PartnersController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        public PartnersController(IPartnerService partnerService) => _partnerService = partnerService;

        // Public — ana səhifə üçün aktiv partnyorlar
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            var result = await _partnerService.GetAllAsync(onlyActive: true);
            return Ok(ApiResponse<IEnumerable<PartnerDto>>.Ok(result));
        }

        // Admin — hamısı (aktiv + qeyri-aktiv)
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _partnerService.GetAllAsync(onlyActive: false);
            return Ok(ApiResponse<IEnumerable<PartnerDto>>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromForm] PartnerCreateDto dto, IFormFile? logo)
        {
            var result = await _partnerService.CreateAsync(dto, logo);
            return StatusCode(201, ApiResponse<PartnerDto>.Ok(result, "Partnyor əlavə edildi."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromForm] PartnerCreateDto dto, IFormFile? logo)
        {
            var result = await _partnerService.UpdateAsync(id, dto, logo);
            return Ok(ApiResponse<PartnerDto>.Ok(result, "Partnyor yeniləndi."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _partnerService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Partnyor silindi."));
        }
    }
}
