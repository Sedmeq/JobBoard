using System;

namespace JobBoard.Core.Entities
{
    /// <summary>
    /// Ana səhifədə göstərilən müştəri rəyləri (admin tərəfindən idarə olunur).
    /// </summary>
    public class Testimonial
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Subtitle { get; set; }   // məs: "One Year With Us"
        public string Message { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
