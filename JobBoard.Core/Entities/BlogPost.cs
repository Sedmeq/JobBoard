using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{

    public class BlogPost
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = "draft";
        public bool IsFeatured { get; set; }
        public bool IsDeleted { get; set; }
        public int ViewCount { get; set; }
        public int ReadTimeMinutes { get; set; }
        public string[]? Tags { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User Author { get; set; } = null!;
        public ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
    }

    public class BlogComment
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public BlogPost BlogPost { get; set; } = null!;
        public User User { get; set; } = null!;
        public BlogComment? ParentComment { get; set; }
        public ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    }
}
