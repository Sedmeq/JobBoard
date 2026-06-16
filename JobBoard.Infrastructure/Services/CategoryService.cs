using JobBoard.Core.DTOs.Categories;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Jobs;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<CategoryDto>> GetAllAsync(bool includeJobCount)
        {
            var query = _db.Categories
                .Include(c => c.Jobs)
                .OrderBy(c => c.SortOrder);

            return await query.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                IconClass = c.IconClass,
                Color = c.Color,
                SortOrder = c.SortOrder,
                JobCount = includeJobCount
                    ? c.Jobs.Count(j => j.Status == "active")
                    : 0
            }).ToListAsync();
        }

        public async Task<PagedResponse<JobListDto>> GetJobsByCategoryAsync(
            string slug, int page, int pageSize, int? currentUserId)
        {
            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug)
                ?? throw new NotFoundException("Kateqoriya tapılmadı.");

            var query = _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.CategoryId == category.Id && j.Status == "active")
                .OrderByDescending(j => j.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new JobListDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Slug = j.Slug,
                    Location = j.Location,
                    IsRemote = j.IsRemote,
                    JobType = j.JobType,
                    ExperienceLevel = j.ExperienceLevel,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    SalaryCurrency = j.SalaryCurrency,
                    IsSalaryVisible = j.IsSalaryVisible,
                    IsFeatured = j.IsFeatured,
                    IsUrgent = j.IsUrgent,
                    Deadline = j.Deadline,
                    CreatedAt = j.CreatedAt,
                    ViewCount = j.ViewCount,
                    ApplicationCount = j.Applications.Count,
                    Company = new JobCompanyDto
                    {
                        Id = j.Company.Id,
                        Name = j.Company.Name,
                        LogoUrl = j.Company.LogoUrl,
                        IsVerified = j.Company.IsVerified
                    },
                    Category = new JobCategoryDto
                    {
                        Id = j.Category.Id,
                        Name = j.Category.Name,
                        IconClass = j.Category.IconClass
                    }
                })
                .ToListAsync();

            return new PagedResponse<JobListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
        {
            var slug = SlugHelper.Generate(dto.Name);
            var exists = await _db.Categories.AnyAsync(c => c.Slug == slug);
            if (exists) slug = $"{slug}-{DateTime.UtcNow.Ticks}";

            var category = new Category
            {
                Name = dto.Name,
                Slug = slug,
                IconClass = dto.IconClass,
                Color = dto.Color,
                SortOrder = dto.SortOrder
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                IconClass = category.IconClass,
                Color = category.Color,
                SortOrder = category.SortOrder,
                JobCount = 0
            };
        }

        public async Task<CategoryDto> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _db.Categories.FindAsync(id)
                ?? throw new NotFoundException("Kateqoriya tapılmadı.");

            category.Name = dto.Name;
            category.IconClass = dto.IconClass;
            category.Color = dto.Color;
            category.SortOrder = dto.SortOrder;

            await _db.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                IconClass = category.IconClass,
                Color = category.Color,
                SortOrder = category.SortOrder
            };
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id)
                ?? throw new NotFoundException("Kateqoriya tapılmadı.");

            var hasJobs = await _db.Jobs.AnyAsync(j => j.CategoryId == id);
            if (hasJobs)
                throw new ConflictException("Bu kateqoriyaya aid ilanlar var. Əvvəlcə onları köçürün.");

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }
}
