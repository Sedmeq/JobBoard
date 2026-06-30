using JobBoard.Core.DTOs.Partners;
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
    public class PartnerService : IPartnerService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public PartnerService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<IEnumerable<PartnerDto>> GetAllAsync(bool onlyActive)
        {
            var query = _db.Partners.AsQueryable();
            if (onlyActive) query = query.Where(p => p.IsActive);
            return await query
                .OrderBy(p => p.SortOrder).ThenByDescending(p => p.Id)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<PartnerDto> CreateAsync(PartnerCreateDto dto, IFormFile? logo)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new BadRequestException("Partnyor adı tələb olunur.");
            if (logo == null)
                throw new BadRequestException("Logo şəkli tələb olunur.");

            var url = await SaveLogoAsync(logo);
            var partner = new Partner
            {
                Name = dto.Name.Trim(),
                Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website.Trim(),
                LogoUrl = url,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _db.Partners.Add(partner);
            await _db.SaveChangesAsync();
            return MapToDto(partner);
        }

        public async Task<PartnerDto> UpdateAsync(int id, PartnerCreateDto dto, IFormFile? logo)
        {
            var partner = await _db.Partners.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Partnyor tapılmadı.");

            if (!string.IsNullOrWhiteSpace(dto.Name)) partner.Name = dto.Name.Trim();
            partner.Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website.Trim();
            partner.SortOrder = dto.SortOrder;
            partner.IsActive = dto.IsActive;

            if (logo != null)
                partner.LogoUrl = await SaveLogoAsync(logo);

            await _db.SaveChangesAsync();
            return MapToDto(partner);
        }

        public async Task DeleteAsync(int id)
        {
            var partner = await _db.Partners.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Partnyor tapılmadı.");
            _db.Partners.Remove(partner);
            await _db.SaveChangesAsync();
        }

        private async Task<string> SaveLogoAsync(IFormFile file)
        {
            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/svg+xml", "image/gif" };
            if (!allowed.Contains(file.ContentType))
                throw new BadRequestException("Yalnız JPEG, PNG, WEBP, SVG və GIF formatları qəbul edilir.");
            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("Şəkil ölçüsü 5MB-dan çox ola bilməz.");

            var uploadsPath = Path.Combine("wwwroot", "uploads", "partners");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);
            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var baseUrl = _config["Storage:BaseUrl"];
            return $"{baseUrl}/uploads/partners/{fileName}";
        }

        private static PartnerDto MapToDto(Partner p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            LogoUrl = p.LogoUrl,
            Website = p.Website,
            SortOrder = p.SortOrder,
            IsActive = p.IsActive
        };
    }
}
