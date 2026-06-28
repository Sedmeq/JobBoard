using JobBoard.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _frontendBaseUrl;
        private readonly string _apiBaseUrl;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _fromEmail = config["Email:FromEmail"]!;
            _fromName = config["Email:FromName"]!;
            // URL-lər konfiqurasiyadan oxunur; yoxdursa lokal default-lara düşür
            _frontendBaseUrl = (config["App:FrontendBaseUrl"] ?? "http://127.0.0.1:5500").TrimEnd('/');
            _apiBaseUrl = (config["App:ApiBaseUrl"] ?? "https://localhost:7135").TrimEnd('/');
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var client = new SmtpClient(_config["Email:SmtpHost"])
            {
                Port = int.Parse(_config["Email:SmtpPort"]!),
                Credentials = new NetworkCredential(
                    _config["Email:SmtpUser"],
                    _config["Email:SmtpPass"]),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }

        public async Task SendEmailVerificationAsync(string toEmail, string name, string token)
        {
            // Verify linki backend endpoint-inə gedir, o da təsdiqdən sonra frontend-ə yönləndirir
            var link = $"{_apiBaseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(token)}";
            var html = $"""
            <h2>Salam, {name}!</h2>
            <p>Hesabınızı təsdiqləmək üçün aşağıdakı linkə klikləyin:</p>
            <a href="{link}" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                Email-i Təsdiqlə
            </a>
            <p>Link 24 saat ərzində etibarlıdır.</p>
        """;
            await SendAsync(toEmail, "Email adresinizi təsdiqləyin", html);
        }

        public async Task SendPasswordResetAsync(string toEmail, string name, string token)
        {
            var link = $"{_frontendBaseUrl}/reset-password.html?token={Uri.EscapeDataString(token)}";
            var html = $"""
            <h2>Salam, {name}!</h2>
            <p>Şifrənizi sıfırlamaq üçün aşağıdakı linkə klikləyin:</p>
            <a href="{link}" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                Şifrəni Sıfırla
            </a>
            <p>Link 1 saat ərzində etibarlıdır. Əgər siz bu tələbi göndərməmisinizsə, bu emaili nəzərə almayın.</p>
        """;
            await SendAsync(toEmail, "Şifrə sıfırlama tələbi", html);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string name)
        {
            var html = $"""
            <h2>Xoş gəldiniz, {name}!</h2>
            <p>JobBoard platformasına qoşulduğunuz üçün təşəkkür edirik.</p>
            <p>İndi iş elanlarına baxmağa başlaya bilərsiniz.</p>
            <a href="{_frontendBaseUrl}/browse-job-grid.html" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                İşlərə Bax
            </a>
        """;
            await SendAsync(toEmail, "JobBoard-a xoş gəldiniz!", html);
        }

        public async Task SendApplicationReceivedAsync(string toEmail, string candidateName, string jobTitle)
        {
            var html = $"""
            <h2>Salam, {candidateName}!</h2>
            <p><strong>{jobTitle}</strong> vəzifəsinə müraciətiniz uğurla qəbul edildi.</p>
            <p>İşəgötürən müraciətinizi nəzərdən keçirdikdən sonra sizinlə əlaqə saxlayacaq.</p>
        """;
            await SendAsync(toEmail, $"Müraciətiniz qəbul edildi: {jobTitle}", html);
        }

        public async Task SendNewApplicationAsync(string toEmail, string employerName, string candidateName, string jobTitle)
        {
            var html = $"""
            <h2>Salam, {employerName}!</h2>
            <p><strong>{candidateName}</strong> adlı namizəd <strong>{jobTitle}</strong> vəzifənizə müraciət etdi.</p>
            <a href="{_frontendBaseUrl}/company-manage-job.html" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                Müraciəti Bax
            </a>
        """;
            await SendAsync(toEmail, $"Yeni müraciət: {jobTitle}", html);
        }

        public async Task SendApplicationStatusChangedAsync(string toEmail, string candidateName, string jobTitle, string status)
        {
            var statusAz = status switch
            {
                "shortlisted" => "Qısa siyahıya alındı",
                "interview" => "Müsahibəyə dəvət edildi",
                "offered" => "Təklif verildi",
                "rejected" => "Rədd edildi",
                _ => status
            };

            var html = $"""
            <h2>Salam, {candidateName}!</h2>
            <p><strong>{jobTitle}</strong> vəzifəsinə müraciətinizin statusu dəyişdi:</p>
            <h3 style="color:#6366f1;">{statusAz}</h3>
            <a href="{_frontendBaseUrl}/jobs-applied-job.html" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                Müraciətlərimə Bax
            </a>
        """;
            await SendAsync(toEmail, $"Müraciət statusu dəyişdi: {jobTitle}", html);
        }

        public async Task SendContactReplyAsync(string toEmail, string name, string originalSubject, string replyMessage)
        {
            var safeReply = replyMessage.Replace("\n", "<br>");
            var html = $"""
            <h2>Salam, {name}!</h2>
            <p>"<strong>{originalSubject}</strong>" mövzusunda bizə yazdığınız mesaja cavabımız:</p>
            <div style="background:#f5f5f7;border-left:4px solid #6366f1;padding:16px 20px;margin:16px 0;border-radius:6px;">
                {safeReply}
            </div>
            <p>JobBoard komandasından sizə təşəkkür edirik. Əlavə sualınız olarsa, bu emailə cavab yaza bilərsiniz.</p>
            <a href="{_frontendBaseUrl}/contact.html" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                Bizimlə Əlaqə
            </a>
        """;
            await SendAsync(toEmail, $"Re: {originalSubject}", html);
        }

        public async Task SendChatStartedAsync(string toEmail, string candidateName, string companyName, string jobTitle, string chatLink)
        {
            var html = $"""
            <h2>Salam, {candidateName}!</h2>
            <p><strong>{companyName}</strong> şirkəti <strong>{jobTitle}</strong> vəzifəsinə müraciətinizlə bağlı sizinlə əlaqə saxlamaq istəyir.</p>
            <p>Söhbətə qoşulmaq və işəgötürənlə yazışmaq üçün aşağıdakı düyməyə klikləyin:</p>
            <a href="{chatLink}" style="background:#2e55fa;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                Söhbəti Aç
            </a>
            <p style="color:#6b7280;font-size:13px;margin-top:16px;">Əgər düymə işləmirsə, bu linki brauzerə kopyalayın:<br>{chatLink}</p>
        """;
            await SendAsync(toEmail, $"{companyName} sizinlə əlaqə saxlayır — {jobTitle}", html);
        }
    }
}
