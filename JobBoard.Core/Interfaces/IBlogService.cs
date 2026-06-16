using JobBoard.Core.DTOs.Blog;
using JobBoard.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    public interface IBlogService
    {
        Task<PagedResponse<BlogPostListDto>> GetPostsAsync(BlogFilterDto filter, bool isAdmin);
        Task<BlogPostDetailDto> GetPostBySlugAsync(string slug, bool isAdmin);
        Task<BlogPostDetailDto> CreatePostAsync(BlogPostCreateDto dto, int authorId);
        Task<BlogPostDetailDto> UpdatePostAsync(int id, BlogPostUpdateDto dto);
        Task DeletePostAsync(int id);
        Task<BlogCommentDto> AddCommentAsync(int postId, int userId, BlogCommentCreateDto dto);
        Task DeleteCommentAsync(int commentId, int userId, string userRole);
        Task<IEnumerable<BlogCategoryStatsDto>> GetCategoriesAsync();
        Task<IEnumerable<BlogTagDto>> GetTagsAsync();
    }
}
