using JobBoard.Core.DTOs.Admin;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Exceptions;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var totalUsers = await _db.Users.CountAsync();
            var totalJobs = await _db.Jobs.CountAsync();
            var totalApplications = await _db.JobApplications.CountAsync();
            var totalCompanies = await _db.Companies.CountAsync();
            var newUsersThisMonth = await _db.Users.CountAsync(u => u.CreatedAt >= monthStart);
            var newJobsThisMonth = await _db.Jobs.CountAsync(j => j.CreatedAt >= monthStart);
            var revenueThisMonth = await _db.Transactions
                .Where(t => t.Status == "completed" && t.CreatedAt >= monthStart)
                .SumAsync(t => t.Amount);

            var jobsByStatus = await _db.Jobs
                .GroupBy(j => j.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var topCategories = await _db.Categories
                .Include(c => c.Jobs)
                .OrderByDescending(c => c.Jobs.Count(j => j.Status == "active"))
                .Take(5)
                .Select(c => new TopCategoryDto
                {
                    Name = c.Name,
                    JobCount = c.Jobs.Count(j => j.Status == "active")
                })
                .ToListAsync();

            return Ok(ApiResponse<AdminDashboardDto>.Ok(new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalJobs = totalJobs,
                TotalApplications = totalApplications,
                TotalCompanies = totalCompanies,
                NewUsersThisMonth = newUsersThisMonth,
                NewJobsThisMonth = newJobsThisMonth,
                RevenueThisMonth = revenueThisMonth,
                JobsByStatus = jobsByStatus,
                TopCategories = topCategories
            }));
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? role,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isBanned,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.Users.IgnoreQueryFilters().AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);

            if (isVerified.HasValue)
                query = query.Where(u => u.IsEmailVerified == isVerified.Value);

            if (isBanned.HasValue)
                query = query.Where(u => u.IsBanned == isBanned.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserListDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsEmailVerified = u.IsEmailVerified,
                    IsActive = u.IsActive,
                    IsBanned = u.IsBanned,
                    BanReason = u.BanReason,
                    BannedAt = u.BannedAt,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<PagedResponse<AdminUserListDto>>.Ok(new PagedResponse<AdminUserListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            }));
        }

        [HttpPatch("users/{id:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] AdminUserStatusDto dto)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse.Ok($"İstifadəçi statusu yeniləndi: {(dto.IsActive ? "aktiv" : "deaktiv")}"));
        }

        /// <summary>İstifadəçini ban edir və ya banını götürür (admin).</summary>
        [HttpPatch("users/{id:int}/ban")]
        public async Task<IActionResult> BanUser(int id, [FromBody] AdminBanUserDto dto)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            // Admin hesabı ban edilə bilməz
            if (user.Role == "admin")
                throw new BadRequestException("Admin hesabı ban edilə bilməz.");

            // Adminin özünü ban etməsinin qarşısını al
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user.Id == currentUserId)
                throw new BadRequestException("Öz hesabınızı ban edə bilməzsiniz.");

            if (dto.IsBanned)
            {
                user.IsBanned = true;
                user.BanReason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim();
                user.BannedAt = DateTime.UtcNow;
                // Ban olunan istifadəçinin aktiv sessiyalarını da ləğv et
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
            }
            else
            {
                user.IsBanned = false;
                user.BanReason = null;
                user.BannedAt = null;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse.Ok(dto.IsBanned
                ? "İstifadəçi ban edildi."
                : "İstifadəçinin banı götürüldü."));
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies(
            [FromQuery] string? keyword,
            [FromQuery] bool? isVerified,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Companies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.Name.Contains(keyword));

            if (isVerified.HasValue)
                query = query.Where(c => c.IsVerified == isVerified.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new AdminCompanyListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Industry = c.Industry,
                    Location = c.Location,
                    JobsCount = c.Jobs.Count(j => !j.IsDeleted),
                    IsVerified = c.IsVerified,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<PagedResponse<AdminCompanyListDto>>.Ok(new PagedResponse<AdminCompanyListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            }));
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Transactions.Include(t => t.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new AdminTransactionListDto
                {
                    Id = t.Id,
                    OrderId = t.InvoiceNumber,
                    TransactionType = t.Type,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    PaymentMethod = t.PaymentMethod,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UserFullName = t.User.FullName
                })
                .ToListAsync();

            return Ok(ApiResponse<PagedResponse<AdminTransactionListDto>>.Ok(new PagedResponse<AdminTransactionListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            }));
        }

        [HttpPatch("companies/{id:int}/verify")]
        public async Task<IActionResult> VerifyCompany(int id, [FromBody] AdminVerifyCompanyDto dto)
        {
            var company = await _db.Companies.FindAsync(id)
                ?? throw new NotFoundException("Şirkət tapılmadı.");

            company.IsVerified = dto.IsVerified;
            company.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse.Ok($"Şirkət {(dto.IsVerified ? "təsdiqləndi" : "təsdiq ləğv edildi")}."));
        }
    }
}
