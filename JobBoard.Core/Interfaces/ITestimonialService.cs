using JobBoard.Core.DTOs.Testimonials;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    public interface ITestimonialService
    {
        Task<IEnumerable<TestimonialDto>> GetAllAsync(bool onlyActive);
        Task<TestimonialDto> CreateAsync(TestimonialCreateDto dto, IFormFile? avatar);
        Task<TestimonialDto> UpdateAsync(int id, TestimonialCreateDto dto, IFormFile? avatar);
        Task DeleteAsync(int id);
    }
}
