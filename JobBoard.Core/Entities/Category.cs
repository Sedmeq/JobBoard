using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }

        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
