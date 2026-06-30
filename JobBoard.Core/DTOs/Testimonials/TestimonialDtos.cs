namespace JobBoard.Core.DTOs.Testimonials
{
    public class TestimonialDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Subtitle { get; set; }
        public string Message { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class TestimonialCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Subtitle { get; set; }
        public string Message { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
