using JobBoard.Core.DTOs.Admin;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    public interface ISiteSettingsService
    {
        /// <summary>Admin üçün tam parametrlər (default-larla birləşdirilmiş).</summary>
        Task<SiteSettingsDto> GetSettingsAsync();

        /// <summary>Admin paneldən parametrləri yeniləyir (upsert).</summary>
        Task UpdateSettingsAsync(SiteSettingsDto dto);

        /// <summary>Contact səhifəsi üçün public məlumatlar.</summary>
        Task<ContactPublicInfoDto> GetPublicContactInfoAsync();

        /// <summary>Tək açarın dəyəri.</summary>
        Task<string?> GetValueAsync(string key);
    }

    public interface IRecaptchaService
    {
        /// <summary>
        /// reCAPTCHA token-ini yoxlayır. reCAPTCHA deaktivdirsə və ya secret konfiqurasiya
        /// olunmayıbsa true qaytarır (yumşaq keçid).
        /// </summary>
        Task<bool> VerifyAsync(string? token);
    }
}
