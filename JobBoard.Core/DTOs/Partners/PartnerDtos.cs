namespace JobBoard.Core.DTOs.Partners
{
    public class PartnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string LogoUrl { get; set; } = null!;
        public string? Website { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class PartnerCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Website { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
