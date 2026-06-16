using JobBoard.Core.DTOs.Admin;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
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
        public ContactController(AppDbContext db) => _db = db;

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ContactCreateDto dto)
        {
            _db.ContactMessages.Add(new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Subject = dto.Subject,
                Message = dto.Message,
                Phone = dto.Phone,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

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
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _db.ContactMessages.OrderByDescending(m => m.CreatedAt);
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
                    CreatedAt = m.CreatedAt
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
    }
}
