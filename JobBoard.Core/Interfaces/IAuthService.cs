using JobBoard.Core.DTOs.Auth;
using JobBoard.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{

    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(int userId, string refreshToken);
        Task<bool> VerifyEmailAsync(string token);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<UserInfoDto> GetMeAsync(int userId);
        Task UpdatePreferencesAsync(int userId, UpdatePreferencesDto dto);
    }

    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }

    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string name, string token);
        Task SendPasswordResetAsync(string toEmail, string name, string token);
        Task SendWelcomeEmailAsync(string toEmail, string name);
        Task SendApplicationReceivedAsync(string toEmail, string candidateName, string jobTitle);
        Task SendNewApplicationAsync(string toEmail, string employerName, string candidateName, string jobTitle);
        Task SendApplicationStatusChangedAsync(string toEmail, string candidateName, string jobTitle, string status);
        Task SendContactReplyAsync(string toEmail, string name, string originalSubject, string replyMessage);
        Task SendChatStartedAsync(string toEmail, string candidateName, string companyName, string jobTitle, string chatLink);
    }
}
