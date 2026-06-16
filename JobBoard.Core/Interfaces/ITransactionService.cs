using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResponse<TransactionDto>> GetTransactionsAsync(int userId, TransactionFilterDto filter);
        Task<TransactionDto> GetByIdAsync(int id, int userId);
        Task<TransactionDto> CreateTransactionAsync(int userId, TransactionCreateDto dto);
        Task<IEnumerable<PlanDto>> GetPlansAsync();
        Task<byte[]> GenerateInvoicePdfAsync(int id, int userId);
    }
}
