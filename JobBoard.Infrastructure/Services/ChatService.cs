using JobBoard.Core.DTOs.Chat;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _db;
        private readonly INotificationPublisher _publisher;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ChatService(
            AppDbContext db,
            INotificationPublisher publisher,
            INotificationService notificationService,
            IEmailService emailService,
            IConfiguration config)
        {
            _db = db;
            _publisher = publisher;
            _notificationService = notificationService;
            _emailService = emailService;
            _config = config;
        }

        public async Task<ChatConversationDto> StartConversationAsync(int employerUserId, int applicationId)
        {
            var app = await _db.JobApplications
                .Include(a => a.Job).ThenInclude(j => j.Company)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new NotFoundException("Müraciət tapılmadı.");

            if (app.Job?.Company == null || app.Job.Company.UserId != employerUserId)
                throw new ForbiddenException("Bu müraciətə cavab vermək icazəniz yoxdur.");

            var convo = await _db.ChatConversations.FirstOrDefaultAsync(c => c.ApplicationId == applicationId);
            var isNew = false;

            if (convo == null)
            {
                convo = new ChatConversation
                {
                    ApplicationId = applicationId,
                    JobId = app.JobId,
                    EmployerUserId = employerUserId,
                    CandidateUserId = app.UserId,
                    Status = "open",
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };
                _db.ChatConversations.Add(convo);
                await _db.SaveChangesAsync();
                isNew = true;
            }
            else if (convo.Status == "closed")
            {
                convo.Status = "open";
                convo.ClosedAt = null;
                await _db.SaveChangesAsync();
            }

            if (isNew)
            {
                var companyName = app.Job.Company.Name;
                await _notificationService.CreateNotificationAsync(
                    app.UserId,
                    "Yeni söhbət",
                    $"{companyName} sizinlə \"{app.Job.Title}\" vəzifəsi üzrə əlaqə saxlayır.",
                    "chat",
                    $"chat.html?conversation={convo.Id}");

                try
                {
                    var frontend = (_config["App:FrontendBaseUrl"] ?? "http://127.0.0.1:5500").TrimEnd('/');
                    var chatLink = $"{frontend}/chat.html?conversation={convo.Id}";
                    await _emailService.SendChatStartedAsync(
                        app.User.Email, app.User.FullName, companyName, app.Job.Title, chatLink);
                }
                catch { /* email xətası söhbəti bloklamamalıdır */ }
            }

            return await BuildConversationDtoAsync(convo, employerUserId);
        }

        public async Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(int userId)
        {
            var convos = await _db.ChatConversations
                .Where(c => c.EmployerUserId == userId || c.CandidateUserId == userId)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            var result = new List<ChatConversationDto>();
            foreach (var c in convos)
                result.Add(await BuildConversationDtoAsync(c, userId));
            return result;
        }

        public async Task<ChatConversationDetailDto> GetConversationAsync(int userId, int conversationId)
        {
            var convo = await _db.ChatConversations.FirstOrDefaultAsync(c => c.Id == conversationId)
                ?? throw new NotFoundException("Söhbət tapılmadı.");

            if (convo.EmployerUserId != userId && convo.CandidateUserId != userId)
                throw new ForbiddenException("Bu söhbətə baxmaq icazəniz yoxdur.");

            // Qarşı tərəfin mesajlarını oxunmuş işarələ
            var unread = await _db.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.SenderUserId != userId && !m.IsRead)
                .ToListAsync();
            if (unread.Any())
            {
                unread.ForEach(m => m.IsRead = true);
                await _db.SaveChangesAsync();
            }

            var baseDto = await BuildConversationDtoAsync(convo, userId);
            var detail = new ChatConversationDetailDto
            {
                Id = baseDto.Id,
                ApplicationId = baseDto.ApplicationId,
                JobId = baseDto.JobId,
                JobTitle = baseDto.JobTitle,
                Status = baseDto.Status,
                CreatedAt = baseDto.CreatedAt,
                LastMessageAt = baseDto.LastMessageAt,
                OtherPartyUserId = baseDto.OtherPartyUserId,
                OtherPartyName = baseDto.OtherPartyName,
                OtherPartyAvatar = baseDto.OtherPartyAvatar,
                LastMessage = baseDto.LastMessage,
                UnreadCount = 0,
                IsEmployer = baseDto.IsEmployer
            };

            var names = await GetNamesAsync(convo.EmployerUserId, convo.CandidateUserId);
            var messages = await _db.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            detail.Messages = messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderUserId = m.SenderUserId,
                SenderName = names.TryGetValue(m.SenderUserId, out var n) ? n : "User",
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            }).ToList();

            return detail;
        }

        public async Task<ChatMessageDto> SendMessageAsync(int userId, int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new BadRequestException("Mesaj boş ola bilməz.");

            var convo = await _db.ChatConversations.FirstOrDefaultAsync(c => c.Id == conversationId)
                ?? throw new NotFoundException("Söhbət tapılmadı.");

            if (convo.EmployerUserId != userId && convo.CandidateUserId != userId)
                throw new ForbiddenException("Bu söhbətə yazmaq icazəniz yoxdur.");

            if (convo.Status == "closed")
                throw new BadRequestException("Bu söhbət bağlanıb, mesaj göndərmək mümkün deyil.");

            var msg = new ChatMessage
            {
                ConversationId = conversationId,
                SenderUserId = userId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.ChatMessages.Add(msg);
            convo.LastMessageAt = msg.SentAt;
            await _db.SaveChangesAsync();

            var names = await GetNamesAsync(convo.EmployerUserId, convo.CandidateUserId);
            var senderName = names.TryGetValue(userId, out var sn) ? sn : "User";
            var recipientId = userId == convo.EmployerUserId ? convo.CandidateUserId : convo.EmployerUserId;

            var dto = new ChatMessageDto
            {
                Id = msg.Id,
                ConversationId = conversationId,
                SenderUserId = userId,
                SenderName = senderName,
                Content = msg.Content,
                SentAt = msg.SentAt,
                IsRead = false
            };

            // Real-time hər iki tərəfə
            await _publisher.PushChatMessageAsync(recipientId, dto);
            await _publisher.PushChatMessageAsync(userId, dto);

            // Qarşı tərəfə bildiriş
            var preview = msg.Content.Length > 80 ? msg.Content.Substring(0, 80) + "…" : msg.Content;
            await _notificationService.CreateNotificationAsync(
                recipientId,
                "Yeni mesaj",
                $"{senderName}: {preview}",
                "chat",
                $"chat.html?conversation={conversationId}");

            return dto;
        }

        public async Task CloseConversationAsync(int employerUserId, int conversationId)
        {
            var convo = await _db.ChatConversations.FirstOrDefaultAsync(c => c.Id == conversationId)
                ?? throw new NotFoundException("Söhbət tapılmadı.");

            if (convo.EmployerUserId != employerUserId)
                throw new ForbiddenException("Yalnız işəgötürən söhbəti bağlaya bilər.");

            convo.Status = "closed";
            convo.ClosedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _publisher.PushChatClosedAsync(convo.CandidateUserId, conversationId);
            await _publisher.PushChatClosedAsync(convo.EmployerUserId, conversationId);

            await _notificationService.CreateNotificationAsync(
                convo.CandidateUserId,
                "Söhbət bağlandı",
                "İşəgötürən söhbəti sona çatdırdı.",
                "chat",
                $"chat.html?conversation={conversationId}");
        }

        // --- Helpers ---

        private async Task<Dictionary<int, string>> GetNamesAsync(params int[] userIds)
        {
            var ids = userIds.Distinct().ToList();
            return await _db.Users.IgnoreQueryFilters()
                .Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);
        }

        private async Task<ChatConversationDto> BuildConversationDtoAsync(ChatConversation c, int userId)
        {
            var isEmployer = userId == c.EmployerUserId;
            var otherId = isEmployer ? c.CandidateUserId : c.EmployerUserId;

            var jobTitle = await _db.Jobs.IgnoreQueryFilters()
                .Where(j => j.Id == c.JobId)
                .Select(j => j.Title)
                .FirstOrDefaultAsync() ?? "Vacancy";

            var other = await _db.Users.IgnoreQueryFilters()
                .Where(u => u.Id == otherId)
                .Select(u => new { u.FullName, u.AvatarUrl })
                .FirstOrDefaultAsync();

            var lastMsg = await _db.ChatMessages
                .Where(m => m.ConversationId == c.Id)
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.Content)
                .FirstOrDefaultAsync();

            var unreadCount = await _db.ChatMessages
                .CountAsync(m => m.ConversationId == c.Id && m.SenderUserId != userId && !m.IsRead);

            return new ChatConversationDto
            {
                Id = c.Id,
                ApplicationId = c.ApplicationId,
                JobId = c.JobId,
                JobTitle = jobTitle,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                LastMessageAt = c.LastMessageAt,
                OtherPartyUserId = otherId,
                OtherPartyName = other?.FullName ?? "User",
                OtherPartyAvatar = other?.AvatarUrl,
                LastMessage = lastMsg,
                UnreadCount = unreadCount,
                IsEmployer = isEmployer
            };
        }
    }
}
