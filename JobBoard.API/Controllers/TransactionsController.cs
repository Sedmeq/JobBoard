using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Transactions;
using JobBoard.Core.Entities;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{

    [ApiController]
    [Route("api/transactions")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionsController(ITransactionService transactionService)
            => _transactionService = transactionService;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter)
        {
            var result = await _transactionService.GetTransactionsAsync(UserId, filter);
            return Ok(ApiResponse<PagedResponse<TransactionDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _transactionService.GetByIdAsync(id, UserId);
            return Ok(ApiResponse<TransactionDto>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> Create([FromBody] TransactionCreateDto dto)
        {
            var result = await _transactionService.CreateTransactionAsync(UserId, dto);
            return StatusCode(201, ApiResponse<TransactionDto>.Ok(result, "Ödəniş uğurla tamamlandı."));
        }

        [HttpGet("{id:int}/invoice")]
        [Authorize]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var bytes = await _transactionService.GenerateInvoicePdfAsync(id, UserId);
            return File(bytes, "text/plain", $"invoice-{id}.txt");
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var result = await _transactionService.GetPlansAsync();
            return Ok(ApiResponse<IEnumerable<PlanDto>>.Ok(result));
        }
    }
}
