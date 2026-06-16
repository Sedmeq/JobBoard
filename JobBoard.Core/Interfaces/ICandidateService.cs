using JobBoard.Core.DTOs.Candidates;
using JobBoard.Core.DTOs.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{

    public interface ICandidateService
    {
        Task<PagedResponse<CandidateListDto>> GetCandidatesAsync(CandidateFilterDto filter);
        Task<CandidateDetailDto> GetByIdAsync(int id);
        Task<CandidateDetailDto> GetMeAsync(int userId);
        Task<CandidateDetailDto> UpdateMeAsync(int userId, CandidateUpdateDto dto);
        Task<WorkExperienceDto> AddExperienceAsync(int userId, WorkExperienceCreateDto dto);
        Task<WorkExperienceDto> UpdateExperienceAsync(int userId, int expId, WorkExperienceCreateDto dto);
        Task DeleteExperienceAsync(int userId, int expId);
        Task<EducationDto> AddEducationAsync(int userId, EducationCreateDto dto);
        Task<EducationDto> UpdateEducationAsync(int userId, int eduId, EducationCreateDto dto);
        Task DeleteEducationAsync(int userId, int eduId);
        Task<string> UploadResumeAsync(int userId, IFormFile file);
        Task DeleteResumeAsync(int userId);
    }
}
