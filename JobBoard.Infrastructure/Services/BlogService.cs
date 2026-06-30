using JobBoard.Core.DTOs.Blog;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class BlogService : IBlogService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public BlogService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (file == null || !allowed.Contains(file.ContentType))
                throw new BadRequestException("Yalnız JPEG, PNG, WEBP və GIF formatları qəbul edilir.");
            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("Şəkil ölçüsü 5MB-dan çox ola bilməz.");

            var uploadsPath = Path.Combine("wwwroot", "uploads", "blog");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);
            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var baseUrl = _config["Storage:BaseUrl"];
            return $"{baseUrl}/uploads/blog/{fileName}";
        }

        public async Task<PagedResponse<BlogPostListDto>> GetPostsAsync(BlogFilterDto filter, bool isAdmin)
        {
            var query = _db.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Comments)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(b => b.Status == "published");
            else if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(b => b.Status == filter.Status);

            if (!string.IsNullOrWhiteSpace(filter.Category))
                query = query.Where(b => b.Category == filter.Category);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(kw) ||
                    (b.Excerpt != null && b.Excerpt.ToLower().Contains(kw)));
            }

            if (filter.IsFeatured.HasValue)
                query = query.Where(b => b.IsFeatured == filter.IsFeatured.Value);

            // Tag value-converter (string[] ↔ ";"-joined) səbəbilə SQL-ə tərcümə olunmur → yaddaşda filtrlə
            if (!string.IsNullOrWhiteSpace(filter.Tag))
            {
                var tag = filter.Tag.Trim();
                var all = await query
                    .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                    .ToListAsync();
                var matched = all
                    .Where(b => b.Tags != null && b.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                var pagedByTag = matched
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(b => MapToListDto(b))
                    .ToList();
                return new PagedResponse<BlogPostListDto>
                {
                    Items = pagedByTag,
                    TotalCount = matched.Count,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(b => MapToListDto(b))
                .ToListAsync();

            return new PagedResponse<BlogPostListDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<BlogPostDetailDto> GetPostBySlugAsync(string slug, bool isAdmin)
        {
            var post = await _db.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Comments.Where(c => c.ParentCommentId == null))
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.Slug == slug)
                ?? throw new NotFoundException("Məqalə tapılmadı.");

            if (!isAdmin && post.Status != "published")
                throw new NotFoundException("Məqalə tapılmadı.");

            post.ViewCount++;
            await _db.SaveChangesAsync();

            var dto = new BlogPostDetailDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                Excerpt = post.Excerpt,
                FeaturedImageUrl = post.FeaturedImageUrl,
                Category = post.Category,
                Status = post.Status,
                IsFeatured = post.IsFeatured,
                ViewCount = post.ViewCount,
                ReadTimeMinutes = post.ReadTimeMinutes,
                Tags = post.Tags?.ToList() ?? [],
                PublishedAt = post.PublishedAt,
                CreatedAt = post.CreatedAt,
                CommentCount = post.Comments.Count,
                Author = new BlogAuthorDto
                {
                    Id = post.Author.Id,
                    FullName = post.Author.FullName,
                    AvatarUrl = post.Author.AvatarUrl
                },
                Comments = post.Comments
                    .Where(c => c.ParentCommentId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => MapCommentToDto(c))
                    .ToList()
            };

            return dto;
        }

        public async Task<BlogPostDetailDto> CreatePostAsync(BlogPostCreateDto dto, int authorId)
        {
            var slug = await SlugHelper.GenerateUniqueBlogSlugAsync(dto.Title, _db);

            var post = new BlogPost
            {
                AuthorId = authorId,
                Title = dto.Title,
                Slug = slug,
                Content = dto.Content,
                Excerpt = dto.Excerpt,
                FeaturedImageUrl = dto.FeaturedImageUrl,
                Category = dto.Category,
                Status = dto.Status,
                IsFeatured = dto.IsFeatured,
                ReadTimeMinutes = dto.ReadTimeMinutes > 0 ? dto.ReadTimeMinutes : EstimateReadTime(dto.Content),
                Tags = dto.Tags.ToArray(),
                PublishedAt = dto.Status == "published" ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.BlogPosts.Add(post);
            await _db.SaveChangesAsync();

            return await GetPostBySlugAsync(post.Slug, true);
        }

        public async Task<BlogPostDetailDto> UpdatePostAsync(int id, BlogPostUpdateDto dto)
        {
            var post = await _db.BlogPosts.FindAsync(id)
                ?? throw new NotFoundException("Məqalə tapılmadı.");

            post.Title = dto.Title;
            post.Slug = await SlugHelper.GenerateUniqueBlogSlugAsync(dto.Title, _db, id);
            post.Content = dto.Content;
            post.Excerpt = dto.Excerpt;
            post.FeaturedImageUrl = dto.FeaturedImageUrl;
            post.Category = dto.Category;
            post.IsFeatured = dto.IsFeatured;
            post.ReadTimeMinutes = dto.ReadTimeMinutes > 0 ? dto.ReadTimeMinutes : EstimateReadTime(dto.Content);
            post.Tags = dto.Tags.ToArray();
            post.UpdatedAt = DateTime.UtcNow;

            if (post.Status != "published" && dto.Status == "published")
                post.PublishedAt = DateTime.UtcNow;

            post.Status = dto.Status;

            await _db.SaveChangesAsync();
            return await GetPostBySlugAsync(post.Slug, true);
        }

        public async Task DeletePostAsync(int id)
        {
            var post = await _db.BlogPosts.FindAsync(id)
                ?? throw new NotFoundException("Məqalə tapılmadı.");

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<BlogCommentDto> AddCommentAsync(int postId, int userId, BlogCommentCreateDto dto)
        {
            var post = await _db.BlogPosts.FindAsync(postId)
                ?? throw new NotFoundException("Məqalə tapılmadı.");

            if (dto.ParentCommentId.HasValue)
            {
                var parent = await _db.BlogComments.FindAsync(dto.ParentCommentId.Value)
                    ?? throw new NotFoundException("Ana şərh tapılmadı.");
                if (parent.BlogPostId != postId)
                    throw new BadRequestException("Şərh bu məqaləyə aid deyil.");
            }

            var comment = new BlogComment
            {
                BlogPostId = postId,
                UserId = userId,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _db.BlogComments.Add(comment);
            await _db.SaveChangesAsync();

            await _db.Entry(comment).Reference(c => c.User).LoadAsync();

            return MapCommentToDto(comment);
        }

        public async Task DeleteCommentAsync(int commentId, int userId, string userRole)
        {
            var comment = await _db.BlogComments.FindAsync(commentId)
                ?? throw new NotFoundException("Şərh tapılmadı.");

            if (userRole != "admin" && comment.UserId != userId)
                throw new ForbiddenException("Bu şərhi silmək icazəniz yoxdur.");

            comment.IsDeleted = true;
            await _db.SaveChangesAsync();
        }

        public async Task<PagedResponse<AdminBlogCommentDto>> GetAllCommentsAsync(int page, int pageSize)
        {
            var query = _db.BlogComments
                .Include(c => c.User)
                .Include(c => c.BlogPost)
                .OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new AdminBlogCommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorName = c.User != null ? c.User.FullName : "User",
                    PostId = c.BlogPostId,
                    PostTitle = c.BlogPost != null ? c.BlogPost.Title : "(deleted)",
                    PostSlug = c.BlogPost != null ? c.BlogPost.Slug : "",
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<AdminBlogCommentDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<BlogCategoryStatsDto>> GetCategoriesAsync()
        {
            return await _db.BlogPosts
                .Where(b => b.Status == "published" && b.Category != null)
                .GroupBy(b => b.Category!)
                .Select(g => new BlogCategoryStatsDto
                {
                    Category = g.Key,
                    PostCount = g.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogTagDto>> GetTagsAsync()
        {
            var posts = await _db.BlogPosts
                .Where(b => b.Status == "published" && b.Tags != null)
                .Select(b => b.Tags!)
                .ToListAsync();

            return posts
                .SelectMany(t => t)
                .GroupBy(t => t)
                .Select(g => new BlogTagDto { Tag = g.Key, Count = g.Count() })
                .OrderByDescending(t => t.Count)
                .Take(20)
                .ToList();
        }

        private static int EstimateReadTime(string content)
        {
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            return Math.Max(1, wordCount / 200);
        }

        private static BlogPostListDto MapToListDto(BlogPost b) => new()
        {
            Id = b.Id,
            Title = b.Title,
            Slug = b.Slug,
            Excerpt = b.Excerpt,
            FeaturedImageUrl = b.FeaturedImageUrl,
            Category = b.Category,
            Status = b.Status,
            IsFeatured = b.IsFeatured,
            ViewCount = b.ViewCount,
            ReadTimeMinutes = b.ReadTimeMinutes,
            CommentCount = b.Comments.Count,
            Tags = b.Tags?.ToList() ?? [],
            PublishedAt = b.PublishedAt,
            CreatedAt = b.CreatedAt,
            Author = new BlogAuthorDto
            {
                Id = b.Author.Id,
                FullName = b.Author.FullName,
                AvatarUrl = b.Author.AvatarUrl
            }
        };

        private static BlogCommentDto MapCommentToDto(BlogComment c) => new()
        {
            Id = c.Id,
            Content = c.Content,
            ParentCommentId = c.ParentCommentId,
            CreatedAt = c.CreatedAt,
            Author = new BlogAuthorDto
            {
                Id = c.User?.Id ?? 0,
                FullName = c.User?.FullName ?? "İstifadəçi",
                AvatarUrl = c.User?.AvatarUrl
            },
            Replies = c.Replies?
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .Select(r => MapCommentToDto(r))
                .ToList() ?? []
        };
    }
}
