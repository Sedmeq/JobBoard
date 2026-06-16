using JobBoard.Core.DTOs.Admin;
using JobBoard.Core.DTOs.Common;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/search")]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SearchController(AppDbContext db) => _db = db;

        [HttpGet("global")]
        public async Task<IActionResult> GlobalSearch([FromQuery] string q, [FromQuery] int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(ApiResponse<GlobalSearchDto>.Ok(new GlobalSearchDto()));

            var kw = q.ToLower();

            var jobs = await _db.Jobs
                .Include(j => j.Company)
                .Where(j => j.Status == "active" && (
                    j.Title.ToLower().Contains(kw) ||
                    j.Company.Name.ToLower().Contains(kw)))
                .Take(limit)
                .Select(j => new SearchJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Slug = j.Slug,
                    Company = j.Company.Name,
                    Location = j.Location
                })
                .ToListAsync();

            var companies = await _db.Companies
                .Where(c => c.Name.ToLower().Contains(kw))
                .Take(limit)
                .Select(c => new SearchCompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    LogoUrl = c.LogoUrl,
                    Industry = c.Industry
                })
                .ToListAsync();

            var candidates = await _db.CandidateProfiles
                .Include(c => c.User)
                .Where(c => c.User.FullName.ToLower().Contains(kw) ||
                            (c.Headline != null && c.Headline.ToLower().Contains(kw)))
                .Take(limit)
                .Select(c => new SearchCandidateDto
                {
                    Id = c.User.Id,
                    FullName = c.User.FullName,
                    Headline = c.Headline,
                    AvatarUrl = c.User.AvatarUrl
                })
                .ToListAsync();

            var blogPosts = await _db.BlogPosts
                .Where(b => b.Status == "published" &&
                            b.Title.ToLower().Contains(kw))
                .Take(limit)
                .Select(b => new SearchBlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    Excerpt = b.Excerpt
                })
                .ToListAsync();

            return Ok(ApiResponse<GlobalSearchDto>.Ok(new GlobalSearchDto
            {
                Jobs = jobs,
                Companies = companies,
                Candidates = candidates,
                BlogPosts = blogPosts
            }));
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions(
            [FromQuery] string q,
            [FromQuery] string type = "jobs")
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(ApiResponse<IEnumerable<string>>.Ok([]));

            var kw = q.ToLower();
            IEnumerable<string> suggestions = type switch
            {
                "companies" => await _db.Companies
                    .Where(c => c.Name.ToLower().Contains(kw))
                    .Select(c => c.Name)
                    .Take(8)
                    .ToListAsync(),
                "skills" => await _db.CandidateSkills
                    .Where(s => s.Name.ToLower().Contains(kw))
                    .Select(s => s.Name)
                    .Distinct()
                    .Take(8)
                    .ToListAsync(),
                _ => await _db.Jobs
                    .Where(j => j.Status == "active" && j.Title.ToLower().Contains(kw))
                    .Select(j => j.Title)
                    .Distinct()
                    .Take(8)
                    .ToListAsync()
            };

            return Ok(ApiResponse<IEnumerable<string>>.Ok(suggestions));
        }
    }
}
