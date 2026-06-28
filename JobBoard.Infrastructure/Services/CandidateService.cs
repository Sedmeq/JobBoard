using JobBoard.Core.DTOs.Candidates;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class CandidateService : ICandidateService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public CandidateService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<PagedResponse<CandidateListDto>> GetCandidatesAsync(CandidateFilterDto filter)
        {
            var query = _db.CandidateProfiles
                .Include(c => c.User)
                .Include(c => c.Skills)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.ToLower();
                query = query.Where(c =>
                    c.User.FullName.ToLower().Contains(kw) ||
                    (c.Headline != null && c.Headline.ToLower().Contains(kw)) ||
                    c.Skills.Any(s => s.Name.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(c =>
                    c.Location != null &&
                    c.Location.ToLower().Contains(filter.Location.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Skills))
            {
                var skills = filter.Skills.Split(',').Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(c =>
                    c.Skills.Any(s => skills.Contains(s.Name.ToLower())));
            }

            if (filter.ExperienceMin.HasValue)
                query = query.Where(c => c.ExperienceYears >= filter.ExperienceMin.Value);

            if (filter.ExperienceMax.HasValue)
                query = query.Where(c => c.ExperienceYears <= filter.ExperienceMax.Value);

            if (filter.IsAvailable.HasValue)
                query = query.Where(c => c.IsAvailable == filter.IsAvailable.Value);

            query = filter.SortBy switch
            {
                "experience" => query.OrderByDescending(c => c.ExperienceYears),
                _ => query.OrderByDescending(c => c.User.CreatedAt)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CandidateListDto
                {
                    Id = c.User.Id,
                    FullName = c.User.FullName,
                    AvatarUrl = c.User.AvatarUrl,
                    Headline = c.Headline,
                    Location = c.Location,
                    ExperienceYears = c.ExperienceYears,
                    IsAvailable = c.IsAvailable,
                    Skills = c.Skills.Select(s => s.Name).ToList(),
                    CreatedAt = c.User.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<CandidateListDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<CandidateDetailDto> GetByIdAsync(int id)
        {
            var profile = await _db.CandidateProfiles
                .Include(c => c.User)
                .Include(c => c.Skills)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.Languages)
                .FirstOrDefaultAsync(c => c.UserId == id)
                ?? throw new NotFoundException("Namizəd tapılmadı.");

            return MapToDetailDto(profile);
        }

        public async Task<CandidateDetailDto> GetMeAsync(int userId)
            => await GetByIdAsync(userId);

        public async Task<CandidateDetailDto> UpdateMeAsync(int userId, CandidateUpdateDto dto)
        {
            var profile = await _db.CandidateProfiles
                .Include(c => c.Skills)
                .Include(c => c.Languages)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Profil tapılmadı.");

            profile.Headline = dto.Headline;
            profile.Summary = dto.Summary;
            profile.Location = dto.Location;
            profile.Website = dto.Website;
            profile.LinkedInUrl = dto.LinkedInUrl;
            profile.GithubUrl = dto.GithubUrl;
            profile.ExperienceYears = dto.ExperienceYears;
            profile.CurrentPosition = dto.CurrentPosition;
            profile.ExpectedSalary = dto.ExpectedSalary;
            profile.IsAvailable = dto.IsAvailable;

            // Skills yenilə
            _db.CandidateSkills.RemoveRange(profile.Skills);
            if (dto.Skills.Any())
            {
                _db.CandidateSkills.AddRange(dto.Skills.Select(s => new CandidateSkill
                {
                    CandidateProfileId = profile.Id,
                    Name = s
                }));
            }

            // Languages yenilə
            _db.CandidateLanguages.RemoveRange(profile.Languages);
            if (dto.Languages.Any())
            {
                _db.CandidateLanguages.AddRange(dto.Languages.Select(l => new CandidateLanguage
                {
                    CandidateProfileId = profile.Id,
                    Name = l.Name,
                    Level = l.Level
                }));
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(userId);
        }

        public async Task<WorkExperienceDto> AddExperienceAsync(int userId, WorkExperienceCreateDto dto)
        {
            var profile = await GetProfileAsync(userId);

            var exp = new WorkExperience
            {
                CandidateProfileId = profile.Id,
                Company = dto.Company,
                Position = dto.Position,
                Location = dto.Location,
                StartDate = dto.StartDate,
                EndDate = dto.IsCurrent ? null : dto.EndDate,
                IsCurrent = dto.IsCurrent,
                Description = dto.Description
            };

            _db.WorkExperiences.Add(exp);
            await _db.SaveChangesAsync();

            return MapExpToDto(exp);
        }

        public async Task<WorkExperienceDto> UpdateExperienceAsync(int userId, int expId, WorkExperienceCreateDto dto)
        {
            var profile = await GetProfileAsync(userId);
            var exp = await _db.WorkExperiences
                .FirstOrDefaultAsync(e => e.Id == expId && e.CandidateProfileId == profile.Id)
                ?? throw new NotFoundException("Təcrübə tapılmadı.");

            exp.Company = dto.Company;
            exp.Position = dto.Position;
            exp.Location = dto.Location;
            exp.StartDate = dto.StartDate;
            exp.EndDate = dto.IsCurrent ? null : dto.EndDate;
            exp.IsCurrent = dto.IsCurrent;
            exp.Description = dto.Description;

            await _db.SaveChangesAsync();
            return MapExpToDto(exp);
        }

        public async Task DeleteExperienceAsync(int userId, int expId)
        {
            var profile = await GetProfileAsync(userId);
            var exp = await _db.WorkExperiences
                .FirstOrDefaultAsync(e => e.Id == expId && e.CandidateProfileId == profile.Id)
                ?? throw new NotFoundException("Təcrübə tapılmadı.");

            _db.WorkExperiences.Remove(exp);
            await _db.SaveChangesAsync();
        }

        public async Task<EducationDto> AddEducationAsync(int userId, EducationCreateDto dto)
        {
            var profile = await GetProfileAsync(userId);

            var edu = new Education
            {
                CandidateProfileId = profile.Id,
                School = dto.School,
                Degree = dto.Degree,
                Field = dto.Field,
                StartYear = dto.StartYear,
                EndYear = dto.IsCurrent ? null : dto.EndYear,
                IsCurrent = dto.IsCurrent,
                Description = dto.Description
            };

            _db.Educations.Add(edu);
            await _db.SaveChangesAsync();

            return MapEduToDto(edu);
        }

        public async Task<EducationDto> UpdateEducationAsync(int userId, int eduId, EducationCreateDto dto)
        {
            var profile = await GetProfileAsync(userId);
            var edu = await _db.Educations
                .FirstOrDefaultAsync(e => e.Id == eduId && e.CandidateProfileId == profile.Id)
                ?? throw new NotFoundException("Təhsil tapılmadı.");

            edu.School = dto.School;
            edu.Degree = dto.Degree;
            edu.Field = dto.Field;
            edu.StartYear = dto.StartYear;
            edu.EndYear = dto.IsCurrent ? null : dto.EndYear;
            edu.IsCurrent = dto.IsCurrent;
            edu.Description = dto.Description;

            await _db.SaveChangesAsync();
            return MapEduToDto(edu);
        }

        public async Task DeleteEducationAsync(int userId, int eduId)
        {
            var profile = await GetProfileAsync(userId);
            var edu = await _db.Educations
                .FirstOrDefaultAsync(e => e.Id == eduId && e.CandidateProfileId == profile.Id)
                ?? throw new NotFoundException("Təhsil tapılmadı.");

            _db.Educations.Remove(edu);
            await _db.SaveChangesAsync();
        }

        public async Task<string> UploadResumeAsync(int userId, IFormFile file)
        {
            var profile = await GetProfileAsync(userId);

            var allowedTypes = new[] { "application/pdf", "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };

            if (!allowedTypes.Contains(file.ContentType))
                throw new BadRequestException("Yalnız PDF, DOC və DOCX formatları qəbul edilir.");

            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("CV ölçüsü 5MB-dan çox ola bilməz.");

            var uploadsPath = Path.Combine("wwwroot", "uploads", "resumes");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var baseUrl = _config["Storage:BaseUrl"];
            var url = $"{baseUrl}/uploads/resumes/{fileName}";

            profile.ResumeUrl = url;
            await _db.SaveChangesAsync();

            return url;
        }

        public async Task DeleteResumeAsync(int userId)
        {
            var profile = await GetProfileAsync(userId);
            profile.ResumeUrl = null;
            await _db.SaveChangesAsync();
        }

        public async Task<string> UploadAvatarAsync(int userId, IFormFile file)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new BadRequestException("Yalnız JPEG, PNG və WEBP formatları qəbul edilir.");

            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("Şəkil ölçüsü 5MB-dan çox ola bilməz.");

            var uploadsPath = Path.Combine("wwwroot", "uploads", "avatars");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var baseUrl = _config["Storage:BaseUrl"];
            var url = $"{baseUrl}/uploads/avatars/{fileName}";

            user.AvatarUrl = url;
            await _db.SaveChangesAsync();

            return url;
        }

        // --- Helpers ---

        private async Task<CandidateProfile> GetProfileAsync(int userId)
            => await _db.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId)
               ?? throw new NotFoundException("Namizəd profili tapılmadı.");

        private static CandidateDetailDto MapToDetailDto(CandidateProfile c) => new()
        {
            Id = c.User.Id,
            FullName = c.User.FullName,
            Email = c.User.Email,
            AvatarUrl = c.User.AvatarUrl,
            Headline = c.Headline,
            Summary = c.Summary,
            Location = c.Location,
            Website = c.Website,
            LinkedInUrl = c.LinkedInUrl,
            GithubUrl = c.GithubUrl,
            ExperienceYears = c.ExperienceYears,
            CurrentPosition = c.CurrentPosition,
            ExpectedSalary = c.ExpectedSalary,
            IsAvailable = c.IsAvailable,
            ResumeUrl = c.ResumeUrl,
            VideoResumeUrl = c.VideoResumeUrl,
            CreatedAt = c.User.CreatedAt,
            Skills = c.Skills.Select(s => s.Name).ToList(),
            WorkExperiences = c.WorkExperiences.OrderByDescending(e => e.StartDate)
                .Select(e => new WorkExperienceDto
                {
                    Id = e.Id,
                    Company = e.Company,
                    Position = e.Position,
                    Location = e.Location,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Description = e.Description
                }).ToList(),
            Educations = c.Educations.OrderByDescending(e => e.StartYear)
                .Select(e => new EducationDto
                {
                    Id = e.Id,
                    School = e.School,
                    Degree = e.Degree,
                    Field = e.Field,
                    StartYear = e.StartYear,
                    EndYear = e.EndYear,
                    IsCurrent = e.IsCurrent,
                    Description = e.Description
                }).ToList(),
            Languages = c.Languages.Select(l => new CandidateLanguageDto
            {
                Name = l.Name,
                Level = l.Level
            }).ToList()
        };

        private static WorkExperienceDto MapExpToDto(WorkExperience e) => new()
        {
            Id = e.Id,
            Company = e.Company,
            Position = e.Position,
            Location = e.Location,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            IsCurrent = e.IsCurrent,
            Description = e.Description
        };

        private static EducationDto MapEduToDto(Education e) => new()
        {
            Id = e.Id,
            School = e.School,
            Degree = e.Degree,
            Field = e.Field,
            StartYear = e.StartYear,
            EndYear = e.EndYear,
            IsCurrent = e.IsCurrent,
            Description = e.Description
        };
    }
}
