using System;

namespace JobBoard.Core.Entities
{
    /// <summary>
    /// Ana səhifədə göstərilən partnyor/şirkət loqoları (admin tərəfindən idarə olunur).
    /// </summary>
    public class Partner
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string LogoUrl { get; set; } = null!;
        public string? Website { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
