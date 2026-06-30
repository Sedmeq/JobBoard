using JobBoard.Core.DTOs.Auth;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        /// <summary>Yeni istifadəçi qeydiyyatı</summary>
        [HttpPost("register")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var message = await _authService.RegisterAsync(dto);
            return StatusCode(201, ApiResponse.Ok(message));
        }

        /// <summary>Giriş — JWT token alın</summary>
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<LoginResponseDto>.Ok(result));
        }

        /// <summary>Google ilə giriş / qeydiyyat (ID token)</summary>
        [HttpPost("google")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var result = await _authService.GoogleLoginAsync(dto);
            return Ok(ApiResponse<LoginResponseDto>.Ok(result));
        }

        /// <summary>Access token yenilə</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return Ok(ApiResponse<LoginResponseDto>.Ok(result));
        }

        /// <summary>Çıxış</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.LogoutAsync(userId, dto.RefreshToken);
            return Ok(ApiResponse.Ok("Uğurla çıxış edildi."));
        }

        /// <summary>Email doğrulama</summary>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            var frontendBaseUrl = (_config["App:FrontendBaseUrl"] ?? "http://127.0.0.1:5500").TrimEnd('/');

            if (!result)
                return Redirect($"{frontendBaseUrl}/login-3.html?verified=false");

            return Redirect($"{frontendBaseUrl}/login-3.html?verified=true");
        }

        /// <summary>Şifrə sıfırlama emaili göndər</summary>
        [HttpPost("forgot-password")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(ApiResponse.Ok("Əgər bu email mövcuddursa, sıfırlama linki göndərildi."));
        }

        /// <summary>Yeni şifrə təyin et</summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(ApiResponse.Ok("Şifrə uğurla dəyişdirildi."));
        }

        /// <summary>Şifrə dəyiş (giriş tələb olunur)</summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.ChangePasswordAsync(userId, dto);
            return Ok(ApiResponse.Ok("Şifrə uğurla dəyişdirildi."));
        }

        /// <summary>Cari istifadəçi məlumatları</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.GetMeAsync(userId);
            return Ok(ApiResponse<UserInfoDto>.Ok(result));
        }

        /// <summary>Tema seçimi (dark/light mode)</summary>
        [HttpPut("preferences")]
        [Authorize]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.UpdatePreferencesAsync(userId, dto);
            return Ok(ApiResponse.Ok("Tərcihlər yeniləndi."));
        }
    }
}
