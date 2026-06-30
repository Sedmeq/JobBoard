using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Auth
{
    internal class AuthDtos
    {
    }

    public class RegisterDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string Role { get; set; } = null!; // "candidate" | "employer"
    }

    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }

    public class GoogleLoginDto
    {
        public string IdToken { get; set; } = null!;
        // İlk dəfə qeydiyyatda rol seçimi (yoxdursa "candidate")
        public string? Role { get; set; }
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class UpdatePreferencesDto
    {
        public bool IsDarkMode { get; set; }
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int ExpiresIn { get; set; }
        public UserInfoDto User { get; set; } = null!;
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public bool IsDarkMode { get; set; }
    }
}
