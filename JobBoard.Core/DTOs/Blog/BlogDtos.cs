using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Blog
{
    public class BlogDtos
    {
    }

    public class BlogPostCreateDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = "draft";
        public bool IsFeatured { get; set; }
        public int ReadTimeMinutes { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    public class BlogPostUpdateDto : BlogPostCreateDto { }

    public class BlogCommentCreateDto
    {
        public string Content { get; set; } = null!;
        public int? ParentCommentId { get; set; }
    }

    public class BlogFilterDto
    {
        public string? Category { get; set; }
        public string? Tag { get; set; }
        public string? Keyword { get; set; }
        public bool? IsFeatured { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BlogAuthorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }

    public class BlogCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public int? ParentCommentId { get; set; }
        public BlogAuthorDto Author { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<BlogCommentDto> Replies { get; set; } = [];
    }

    public class BlogPostListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = null!;
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public int ReadTimeMinutes { get; set; }
        public int CommentCount { get; set; }
        public List<string> Tags { get; set; } = [];
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public BlogAuthorDto Author { get; set; } = null!;
    }

    public class BlogPostDetailDto : BlogPostListDto
    {
        public string Content { get; set; } = null!;
        public List<BlogCommentDto> Comments { get; set; } = [];
    }

    public class BlogCategoryStatsDto
    {
        public string Category { get; set; } = null!;
        public int PostCount { get; set; }
    }

    public class BlogTagDto
    {
        public string Tag { get; set; } = null!;
        public int Count { get; set; }
    }

    public class AdminBlogCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public int PostId { get; set; }
        public string PostTitle { get; set; } = null!;
        public string PostSlug { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
