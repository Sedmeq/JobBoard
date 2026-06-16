using JobBoard.Core.DTOs.Categories;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync(bool includeJobCount);
        Task<PagedResponse<JobListDto>> GetJobsByCategoryAsync(string slug, int page, int pageSize, int? currentUserId);
        Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
        Task<CategoryDto> UpdateAsync(int id, CategoryUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
