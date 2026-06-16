using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{

    public interface IJobService
    {
        Task<PagedResponse<JobListDto>> GetJobsAsync(JobFilterDto filter, int? currentUserId);
        Task<JobDetailDto> GetJobByIdAsync(int id, int? currentUserId, string? ipAddress);
        Task<JobDetailDto> GetJobBySlugAsync(string slug, int? currentUserId, string? ipAddress);
        Task<IEnumerable<JobListDto>> GetFeaturedJobsAsync();
        Task<IEnumerable<JobListDto>> GetRecentJobsAsync(int count);
        Task<IEnumerable<JobListDto>> GetRelatedJobsAsync(int jobId);
        Task<JobDetailDto> CreateJobAsync(JobCreateDto dto, int userId);
        Task<JobDetailDto> UpdateJobAsync(int id, JobUpdateDto dto, int userId, string userRole);
        Task DeleteJobAsync(int id, int userId, string userRole);
        Task UpdateJobStatusAsync(int id, string status, int userId, string userRole);
        Task UpdateJobFeaturedAsync(int id, bool isFeatured);
        Task<PagedResponse<JobListDto>> GetMyJobsAsync(int userId, string? status, int page, int pageSize);
    }
}
