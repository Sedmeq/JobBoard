using JobBoard.Core.DTOs.Partners;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    public interface IPartnerService
    {
        Task<IEnumerable<PartnerDto>> GetAllAsync(bool onlyActive);
        Task<PartnerDto> CreateAsync(PartnerCreateDto dto, IFormFile? logo);
        Task<PartnerDto> UpdateAsync(int id, PartnerCreateDto dto, IFormFile? logo);
        Task DeleteAsync(int id);
    }
}
