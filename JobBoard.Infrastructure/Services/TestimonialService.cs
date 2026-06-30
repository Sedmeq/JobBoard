using JobBoard.Core.DTOs.Testimonials;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{
    public class TestimonialService : ITestimonialService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public TestimonialService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<IEnumerable<TestimonialDto>> GetAllAsync(bool onlyActive)
        {
            var query = _db.Testimonials.AsQueryable();
            if (onlyActive) query = query.Where(t => t.IsActive);
            return await query
                .OrderBy(t => t.SortOrder).ThenByDescending(t => t.Id)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<TestimonialDto> CreateAsync(TestimonialCreateDto dto, IFormFile? avatar)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new BadRequestException("Ad tələb olunur.");
            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new BadRequestException("Rəy mətni tələb olunur.");

            var t = new Testimonial
            {
                Name = dto.Name.Trim(),
                Subtitle = string.IsNullOrWhiteSpace(dto.Subtitle) ? null : dto.Subtitle.Trim(),
                Message = dto.Message.Trim(),
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            if (avatar != null) t.AvatarUrl = await SaveAvatarAsync(avatar);

            _db.Testimonials.Add(t);
            await _db.SaveChangesAsync();
            return MapToDto(t);
        }

        public async Task<TestimonialDto> UpdateAsync(int id, TestimonialCreateDto dto, IFormFile? avatar)
        {
            var t = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("Rəy tapılmadı.");

            if (!string.IsNullOrWhiteSpace(dto.Name)) t.Name = dto.Name.Trim();
            t.Subtitle = string.IsNullOrWhiteSpace(dto.Subtitle) ? null : dto.Subtitle.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Message)) t.Message = dto.Message.Trim();
            t.SortOrder = dto.SortOrder;
            t.IsActive = dto.IsActive;

            if (avatar != null) t.AvatarUrl = await SaveAvatarAsync(avatar);

            await _db.SaveChangesAsync();
            return MapToDto(t);
        }

        public async Task DeleteAsync(int id)
        {
            var t = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("Rəy tapılmadı.");
            _db.Testimonials.Remove(t);
            await _db.SaveChangesAsync();
        }

        private async Task<string> SaveAvatarAsync(IFormFile file)
        {
            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(file.ContentType))
                throw new BadRequestException("Yalnız JPEG, PNG, WEBP və GIF formatları qəbul edilir.");
            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("Şəkil ölçüsü 5MB-dan çox ola bilməz.");

            var uploadsPath = Path.Combine("wwwroot", "uploads", "testimonials");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);
            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var baseUrl = _config["Storage:BaseUrl"];
            return $"{baseUrl}/uploads/testimonials/{fileName}";
        }

        private static TestimonialDto MapToDto(Testimonial t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            Subtitle = t.Subtitle,
            Message = t.Message,
            AvatarUrl = t.AvatarUrl,
            SortOrder = t.SortOrder,
            IsActive = t.IsActive
        };
    }
}
