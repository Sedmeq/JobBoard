using System;

namespace JobBoard.Core.Entities
{
    /// <summary>
    /// Açar-dəyər prinsipi ilə sayt parametrləri (əlaqə məlumatı, xəritə, reCAPTCHA və s.).
    /// Admin paneldən idarə olunur.
    /// </summary>
    public class SiteSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string? Value { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
