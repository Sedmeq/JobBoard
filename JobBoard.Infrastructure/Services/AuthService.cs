using JobBoard.Core.DTOs.Auth;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Core.Settings;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwt;

        public AuthService(
            AppDbContext db,
            ITokenService tokenService,
            IEmailService emailService,
            IOptions<JwtSettings> jwt)
        {
            _db = db;
            _tokenService = tokenService;
            _emailService = emailService;
            _jwt = jwt.Value;
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                throw new ConflictException("Bu email artıq istifadə olunur.");

            var verificationToken = Guid.NewGuid().ToString();

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);

            // Role-a görə profil yarat
            if (dto.Role == "candidate")
                _db.CandidateProfiles.Add(new CandidateProfile { User = user });
            else if (dto.Role == "employer")
                _db.Companies.Add(new Company { User = user, Name = dto.FullName, CreatedAt = DateTime.UtcNow });

            await _db.SaveChangesAsync();

            await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, verificationToken);

            return "Doğrulama emaili göndərildi. Zəhmət olmasa emailinizi yoxlayın.";
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedException("Email və ya şifrə yanlışdır.");

            if (!user.IsEmailVerified)
                throw new UnauthorizedException("Zəhmət olmasa əvvəlcə email adresinizi doğrulayın.");

            if (!user.IsActive)
                throw new UnauthorizedException("Hesabınız deaktiv edilib.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var expiryDays = dto.RememberMe ? 30 : _jwt.RefreshTokenExpiryDays;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwt.AccessTokenExpiryMinutes * 60,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    AvatarUrl = user.AvatarUrl,
                    IsDarkMode = user.IsDarkMode
                }
            };
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token etibarsızdır və ya müddəti bitib.");

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = _jwt.AccessTokenExpiryMinutes * 60,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    AvatarUrl = user.AvatarUrl,
                    IsDarkMode = user.IsDarkMode
                }
            };
        }

        public async Task LogoutAsync(int userId, string refreshToken)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null && user.RefreshToken == refreshToken)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null) return false;

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

            return true;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

            // Security: həmişə 200 qaytar, user tapılmasa belə
            if (user == null) return;

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _emailService.SendPasswordResetAsync(user.Email, user.FullName, user.PasswordResetToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);

            if (user == null)
                throw new BadRequestException("Token etibarsızdır.");

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                throw new BadRequestException("Token müddəti bitib. Yenidən sıfırlama tələbi göndərin.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.RefreshToken = null; // Bütün sessiyaları ləğv et
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new BadRequestException("Cari şifrə yanlışdır.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task<UserInfoDto> GetMeAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            return new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                IsDarkMode = user.IsDarkMode
            };
        }

        public async Task UpdatePreferencesAsync(int userId, UpdatePreferencesDto dto)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            user.IsDarkMode = dto.IsDarkMode;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
