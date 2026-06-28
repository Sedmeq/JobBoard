using JobBoard.Core.DTOs.Admin;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/contact")]
    [Produces("application/json")]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ISiteSettingsService _settingsService;
        private readonly IRecaptchaService _recaptchaService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            AppDbContext db,
            INotificationService notificationService,
            IEmailService emailService,
            ISiteSettingsService settingsService,
            IRecaptchaService recaptchaService,
            ILogger<ContactController> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _emailService = emailService;
            _settingsService = settingsService;
            _recaptchaService = recaptchaService;
            _logger = logger;
        }

        /// <summary>
        /// Contact səhifəsi üçün public məlumatlar (Address, Email, Phone, Working Hours,
        /// Google Maps embed URL və reCAPTCHA site key). Admin paneldən idarə olunur.
        /// </summary>
        [HttpGet("info")]
        public async Task<IActionResult> GetContactInfo()
        {
            var info = await _settingsService.GetPublicContactInfoAsync();
            return Ok(ApiResponse<ContactPublicInfoDto>.Ok(info));
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ContactCreateDto dto)
        {
            // reCAPTCHA yoxlaması (admin paneldə aktivdirsə)
            var captchaOk = await _recaptchaService.VerifyAsync(dto.RecaptchaToken);
            if (!captchaOk)
                throw new BadRequestException("reCAPTCHA təsdiqlənmədi. Zəhmət olmasa yenidən cəhd edin.");

            var message = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Subject = dto.Subject,
                Message = dto.Message,
                Phone = dto.Phone,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.ContactMessages.Add(message);
            await _db.SaveChangesAsync();

            // Bütün aktiv admin-lərə bildiriş (persist + SignalR real-time)
            var adminIds = await _db.Users
                .Where(u => u.Role == "admin" && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var adminId in adminIds)
            {
                try
                {
                    await _notificationService.CreateNotificationAsync(
                        adminId,
                        "Yeni əlaqə mesajı",
                        $"{dto.Name}: {dto.Subject}",
                        "contact_message",
                        "admin-messages.html");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Admin bildirişi göndərilə bilmədi (adminId: {AdminId})", adminId);
                }
            }

            return Ok(ApiResponse.Ok("Mesajınız göndərildi. Tezliklə əlaqə saxlayacağıq."));
        }

        [HttpPost("newsletter")]
        public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscribeDto dto)
        {
            var exists = await _db.NewsletterSubscribers
                .AnyAsync(n => n.Email == dto.Email.ToLower());

            if (exists)
                throw new ConflictException("Bu email artıq abunədir.");

            _db.NewsletterSubscribers.Add(new NewsletterSubscriber
            {
                Email = dto.Email.ToLower(),
                Name = dto.Name,
                UnsubscribeToken = Guid.NewGuid().ToString(),
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return StatusCode(201, ApiResponse.Ok("Newsletter abunəliyiniz aktivdir."));
        }

        [HttpDelete("newsletter/unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] NewsletterUnsubscribeDto dto)
        {
            var subscriber = await _db.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Email == dto.Email.ToLower() &&
                                          n.UnsubscribeToken == dto.Token)
                ?? throw new NotFoundException("Abunəlik tapılmadı.");

            _db.NewsletterSubscribers.Remove(subscriber);
            await _db.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Abunəliyiniz ləğv edildi."));
        }

        [HttpGet("messages")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetMessages(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? filter = null)
        {
            IQueryable<ContactMessage> baseQuery = _db.ContactMessages;
            if (filter == "unread") baseQuery = baseQuery.Where(m => !m.IsRead);
            else if (filter == "replied") baseQuery = baseQuery.Where(m => m.IsReplied);
            else if (filter == "pending") baseQuery = baseQuery.Where(m => !m.IsReplied);

            var query = baseQuery.OrderByDescending(m => m.CreatedAt);
            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ContactMessageDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Subject = m.Subject,
                    Message = m.Message,
                    Phone = m.Phone,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt,
                    IsReplied = m.IsReplied,
                    ReplyMessage = m.ReplyMessage,
                    RepliedAt = m.RepliedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<PagedResponse<ContactMessageDto>>.Ok(
                new PagedResponse<ContactMessageDto>
                {
                    Items = items,
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize
                }));
        }

        [HttpGet("messages/unread-count")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _db.ContactMessages.CountAsync(m => !m.IsRead);
            return Ok(ApiResponse<int>.Ok(count));
        }

        [HttpPatch("messages/{id:int}/read")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var message = await _db.ContactMessages.FindAsync(id)
                ?? throw new NotFoundException("Mesaj tapılmadı.");

            message.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Mesaj oxundu olaraq işarələndi."));
        }

        /// <summary>
        /// Admin əlaqə mesajına cavab verir: email göndərilir, qeyd saxlanılır,
        /// göndərən qeydiyyatlı istifadəçidirsə SignalR ilə real-time bildiriş göndərilir.
        /// </summary>
        [HttpPost("messages/{id:int}/reply")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Reply(int id, [FromBody] ContactReplyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new BadRequestException("Cavab mesajı boş ola bilməz.");

            var message = await _db.ContactMessages.FindAsync(id)
                ?? throw new NotFoundException("Mesaj tapılmadı.");

            // Göndərənin mailinə cavab
            await _emailService.SendContactReplyAsync(
                message.Email, message.Name, message.Subject, dto.Message);

            message.IsReplied = true;
            message.ReplyMessage = dto.Message;
            message.RepliedAt = DateTime.UtcNow;
            message.IsRead = true;
            await _db.SaveChangesAsync();

            // Göndərən qeydiyyatlı istifadəçidirsə → real-time bildiriş
            var sender = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == message.Email);
            if (sender != null)
            {
                try
                {
                    await _notificationService.CreateNotificationAsync(
                        sender.Id,
                        "Mesajınıza cavab verildi",
                        $"\"{message.Subject}\" mövzusundakı mesajınıza cavab göndərildi.",
                        "contact_reply",
                        null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cavab bildirişi göndərilə bilmədi (userId: {UserId})", sender.Id);
                }
            }

            return Ok(ApiResponse.Ok("Cavab göndərildi."));
        }
    }
}
