using JobBoard.Core.DTOs.Admin;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.API.Controllers
{
    [ApiController]
    [Route("api/admin/settings")]
    [Authorize(Roles = "admin")]
    [Produces("application/json")]
    public class SettingsController : ControllerBase
    {
        private readonly ISiteSettingsService _settings;
        public SettingsController(ISiteSettingsService settings) => _settings = settings;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dto = await _settings.GetSettingsAsync();
            return Ok(ApiResponse<SiteSettingsDto>.Ok(dto));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SiteSettingsDto dto)
        {
            await _settings.UpdateSettingsAsync(dto);
            return Ok(ApiResponse.Ok("Parametrlər yeniləndi."));
        }
    }
}
