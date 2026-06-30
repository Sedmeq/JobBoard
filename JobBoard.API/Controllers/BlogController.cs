using JobBoard.Core.DTOs.Blog;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/blog")]
    [Produces("application/json")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        public BlogController(IBlogService blogService) => _blogService = blogService;

        private int? CurrentUserId =>
            User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

        private bool IsAdmin => User.IsInRole("admin");

        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts([FromQuery] BlogFilterDto filter)
        {
            var result = await _blogService.GetPostsAsync(filter, IsAdmin);
            return Ok(ApiResponse<PagedResponse<BlogPostListDto>>.Ok(result));
        }

        [HttpGet("posts/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _blogService.GetPostBySlugAsync(slug, IsAdmin);
            return Ok(ApiResponse<BlogPostDetailDto>.Ok(result));
        }

        [HttpPost("posts")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] BlogPostCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _blogService.CreatePostAsync(dto, userId);
            return StatusCode(201, ApiResponse<BlogPostDetailDto>.Ok(result, "Məqalə yaradıldı."));
        }

        [HttpPut("posts/{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogPostUpdateDto dto)
        {
            var result = await _blogService.UpdatePostAsync(id, dto);
            return Ok(ApiResponse<BlogPostDetailDto>.Ok(result, "Məqalə yeniləndi."));
        }

        [HttpDelete("posts/{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _blogService.DeletePostAsync(id);
            return Ok(ApiResponse.Ok("Məqalə silindi."));
        }

        [HttpPost("posts/{postId:int}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, [FromBody] BlogCommentCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _blogService.AddCommentAsync(postId, userId, dto);
            return StatusCode(201, ApiResponse<BlogCommentDto>.Ok(result));
        }

        [HttpDelete("comments/{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            await _blogService.DeleteCommentAsync(id, userId, role);
            return Ok(ApiResponse.Ok("Şərh silindi."));
        }

        [HttpGet("comments")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllComments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _blogService.GetAllCommentsAsync(page, pageSize);
            return Ok(ApiResponse<PagedResponse<AdminBlogCommentDto>>.Ok(result));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _blogService.GetCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<BlogCategoryStatsDto>>.Ok(result));
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetTags()
        {
            var result = await _blogService.GetTagsAsync();
            return Ok(ApiResponse<IEnumerable<BlogTagDto>>.Ok(result));
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var url = await _blogService.UploadImageAsync(file);
            return Ok(ApiResponse<object>.Ok(new { url }, "Şəkil yükləndi."));
        }
    }
}
