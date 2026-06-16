using JobBoard.Core.DTOs.Admin;
using JobBoard.Core.DTOs.Common;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/stats")]
    [Produces("application/json")]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public StatsController(AppDbContext db) => _db = db;

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicStats()
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);

            var totalJobs = await _db.Jobs.CountAsync(j => j.Status == "active");
            var totalCandidates = await _db.Users.CountAsync(u => u.Role == "candidate");
            var totalCompanies = await _db.Companies.CountAsync();
            var totalHired = await _db.JobApplications.CountAsync(a => a.Status == "offered");
            var newJobsThisWeek = await _db.Jobs.CountAsync(j => j.CreatedAt >= weekAgo);

            var topLocations = await _db.Jobs
                .Where(j => j.Status == "active")
                .GroupBy(j => j.Location)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            var topSkills = await _db.JobSkills
                .GroupBy(s => s.Name)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => g.Key)
                .ToListAsync();

            return Ok(ApiResponse<PublicStatsDto>.Ok(new PublicStatsDto
            {
                TotalJobs = totalJobs,
                TotalCandidates = totalCandidates,
                TotalCompanies = totalCompanies,
                TotalHired = totalHired,
                NewJobsThisWeek = newJobsThisWeek,
                TopLocations = topLocations,
                TopSkills = topSkills
            }));
        }
    }
}
