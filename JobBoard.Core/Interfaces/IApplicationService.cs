using JobBoard.Core.DTOs.Applications;
using JobBoard.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{

    public interface IApplicationService
    {
        Task<ApplicationListDto> CreateAsync(ApplicationCreateDto dto, int userId);
        Task<PagedResponse<ApplicationListDto>> GetMyApplicationsAsync(int userId, ApplicationFilterDto filter);
        Task<ApplicationListDto> GetByIdAsync(int id, int userId, string userRole);
        Task UpdateStatusAsync(int id, ApplicationStatusUpdateDto dto, int userId, string userRole);
        Task WithdrawAsync(int id, int userId);
        Task<ApplicationStatsDto> GetStatsAsync(int userId);
        Task<PagedResponse<ApplicationListDto>> GetJobApplicantsAsync(int jobId, int userId, string userRole, ApplicationFilterDto filter);
    }
}
