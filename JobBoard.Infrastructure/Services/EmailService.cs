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

        public EmailService(IConfiguration config)
        {
            _config = config;
            _fromEmail = config["Email:FromEmail"]!;
            _fromName = config["Email:FromName"]!;
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
            var link = $"https://localhost:5001/api/auth/verify-email?token={token}";
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
            var link = $"http://localhost:3000/reset-password?token={token}";
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
            <a href="http://localhost:3000/jobs" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
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
            <a href="http://localhost:3000/employer/applications" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
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
            <a href="http://localhost:3000/candidate/applications" style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                Müraciətlərimə Bax
            </a>
        """;
            await SendAsync(toEmail, $"Müraciət statusu dəyişdi: {jobTitle}", html);
        }
    }
}
