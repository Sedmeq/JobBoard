using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Companies;
using Microsoft.AspNetCore.Http;
using JobBoard.Core.DTOs.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{

    public interface ICompanyService
    {
        Task<PagedResponse<CompanyListDto>> GetCompaniesAsync(CompanyFilterDto filter);
        Task<CompanyDetailDto> GetCompanyByIdAsync(int id, int? currentUserId);
        Task<CompanyDetailDto> GetMyCompanyAsync(int userId);
        Task<CompanyDetailDto> UpdateMyCompanyAsync(int userId, CompanyUpdateDto dto);
        Task<string> UploadLogoAsync(int userId, IFormFile file);
        Task<string> UploadCoverAsync(int userId, IFormFile file);
        Task<PagedResponse<CompanyReviewDto>> GetReviewsAsync(int companyId, int page, int pageSize);
        Task AddReviewAsync(int companyId, int userId, CompanyReviewCreateDto dto);
        Task<IEnumerable<CompanyListDto>> GetFeaturedCompaniesAsync();
    }
}
