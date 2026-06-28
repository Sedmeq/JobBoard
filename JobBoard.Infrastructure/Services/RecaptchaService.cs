using JobBoard.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{
    public class RecaptchaService : IRecaptchaService
    {
        private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ISiteSettingsService _settings;

        public RecaptchaService(HttpClient http, IConfiguration config, ISiteSettingsService settings)
        {
            _http = http;
            _config = config;
            _settings = settings;
        }

        public async Task<bool> VerifyAsync(string? token)
        {
            // Admin paneldə reCAPTCHA aktivdirmi?
            var enabled = (await _settings.GetValueAsync(SiteSettingsService.KeyRecaptchaEnabled)) == "true";
            var secret = _config["ReCaptcha:SecretKey"];

            // Deaktiv və ya secret yoxdursa → yumşaq keçid (təsdiqlənmiş sayılır)
            if (!enabled || string.IsNullOrWhiteSpace(secret))
                return true;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var response = await _http.PostAsync(VerifyUrl, new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["secret"] = secret!,
                        ["response"] = token!
                    }));

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty("success", out var success) &&
                       success.GetBoolean();
            }
            catch
            {
                // Google-a çıxış alınmadısa istifadəçini bloklamamaq üçün false qaytarırıq
                return false;
            }
        }
    }
}
