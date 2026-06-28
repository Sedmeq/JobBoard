using JobBoard.Core.DTOs.Admin;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{
    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly AppDbContext _db;
        public SiteSettingsService(AppDbContext db) => _db = db;

        // Parametr açarları
        public const string KeyAddress = "contact.address";
        public const string KeyEmail = "contact.email";
        public const string KeyPhone = "contact.phone";
        public const string KeyWorkingHours = "contact.workingHours";
        public const string KeyMapEmbedUrl = "map.embedUrl";
        public const string KeyRecaptchaEnabled = "recaptcha.enabled";
        public const string KeyRecaptchaSiteKey = "recaptcha.siteKey";

        // Default dəyərlər (DB-də açar yoxdursa istifadə olunur)
        private static readonly Dictionary<string, string> Defaults = new()
        {
            [KeyAddress] = "123 West Street, Melbourne Victoria 3000 Australia",
            [KeyEmail] = "info@jobboard.com",
            [KeyPhone] = "+91 987 654 3210",
            [KeyWorkingHours] = "Mon - Fri: 09:00 - 18:00",
            [KeyMapEmbedUrl] = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d227748.3825624477!2d75.65046970649679!3d26.88544791796718!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x396c4adf4c57e281%3A0xce1c63a0cf22e09!2sJaipur%2C+Rajasthan!5e0!3m2!1sen!2sin!4v1500819483219",
            [KeyRecaptchaEnabled] = "false",
            [KeyRecaptchaSiteKey] = ""
        };

        private async Task<Dictionary<string, string?>> GetAllAsync()
        {
            var db = await _db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
            var result = new Dictionary<string, string?>();
            foreach (var kv in Defaults)
                result[kv.Key] = db.TryGetValue(kv.Key, out var v) ? v : kv.Value;
            return result;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null) return setting.Value;
            return Defaults.TryGetValue(key, out var v) ? v : null;
        }

        public async Task<SiteSettingsDto> GetSettingsAsync()
        {
            var all = await GetAllAsync();
            return new SiteSettingsDto
            {
                ContactAddress = all[KeyAddress],
                ContactEmail = all[KeyEmail],
                ContactPhone = all[KeyPhone],
                ContactWorkingHours = all[KeyWorkingHours],
                MapEmbedUrl = all[KeyMapEmbedUrl],
                RecaptchaEnabled = all[KeyRecaptchaEnabled] == "true",
                RecaptchaSiteKey = all[KeyRecaptchaSiteKey]
            };
        }

        public async Task<ContactPublicInfoDto> GetPublicContactInfoAsync()
        {
            var all = await GetAllAsync();
            return new ContactPublicInfoDto
            {
                Address = all[KeyAddress],
                Email = all[KeyEmail],
                Phone = all[KeyPhone],
                WorkingHours = all[KeyWorkingHours],
                MapEmbedUrl = all[KeyMapEmbedUrl],
                RecaptchaEnabled = all[KeyRecaptchaEnabled] == "true",
                RecaptchaSiteKey = all[KeyRecaptchaSiteKey]
            };
        }

        public async Task UpdateSettingsAsync(SiteSettingsDto dto)
        {
            var map = new Dictionary<string, string?>
            {
                [KeyAddress] = dto.ContactAddress,
                [KeyEmail] = dto.ContactEmail,
                [KeyPhone] = dto.ContactPhone,
                [KeyWorkingHours] = dto.ContactWorkingHours,
                [KeyMapEmbedUrl] = dto.MapEmbedUrl,
                [KeyRecaptchaEnabled] = dto.RecaptchaEnabled ? "true" : "false",
                [KeyRecaptchaSiteKey] = dto.RecaptchaSiteKey
            };

            var existing = await _db.SiteSettings
                .Where(s => map.Keys.Contains(s.Key))
                .ToListAsync();

            foreach (var kv in map)
            {
                var row = existing.FirstOrDefault(s => s.Key == kv.Key);
                if (row != null)
                {
                    row.Value = kv.Value;
                    row.UpdatedAt = System.DateTime.UtcNow;
                }
                else
                {
                    _db.SiteSettings.Add(new SiteSetting
                    {
                        Key = kv.Key,
                        Value = kv.Value,
                        UpdatedAt = System.DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
