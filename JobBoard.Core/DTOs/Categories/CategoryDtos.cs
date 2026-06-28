using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Categories
{
    internal class CategoryDtos
    {
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
        public int JobCount { get; set; }
    }

    public class CategoryCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        // Frontend "icon" adı ilə göndərir — uyğunluq üçün alias
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
    }

    public class CategoryUpdateDto : CategoryCreateDto { }
}
