using JobBoard.Core.DTOs.Common;
using JobBoard.Core.DTOs.Transactions;
using JobBoard.Core.Entities;
using JobBoard.Core.Exceptions;
using JobBoard.Core.Interfaces;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _db;

        private static readonly List<PlanDto> Plans =
        [
            new() {
            Id = "basic", Name = "Basic", Price = 49, Currency = "USD",
            Features = ["1 featured job (30 gün)", "Standart dəstək", "Əsas statistika"],
            IsPopular = false
        },
        new() {
            Id = "standard", Name = "Standard", Price = 99, Currency = "USD",
            Features = ["3 featured job", "CV giriş (aylıq 10)", "Prioritet dəstək", "Ətraflı statistika"],
            IsPopular = true
        },
        new() {
            Id = "premium", Name = "Premium", Price = 199, Currency = "USD",
            Features = ["Limitsiz featured job", "Limitsiz CV girişi", "Dədiket menencer", "API giriş"],
            IsPopular = false
        }
        ];

        public TransactionService(AppDbContext db) => _db = db;

        public async Task<PagedResponse<TransactionDto>> GetTransactionsAsync(
            int userId, TransactionFilterDto filter)
        {
            var query = _db.Transactions
                .Where(t => t.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.Type))
                query = query.Where(t => t.Type == filter.Type);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(t => t.Status == filter.Status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => MapToDto(t))
                .ToListAsync();

            return new PagedResponse<TransactionDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<TransactionDto> GetByIdAsync(int id, int userId)
        {
            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)
                ?? throw new NotFoundException("Əməliyyat tapılmadı.");

            return MapToDto(transaction);
        }

        public async Task<TransactionDto> CreateTransactionAsync(int userId, TransactionCreateDto dto)
        {
            var validTypes = new[] { "featured_job", "cv_access", "subscription" };
            if (!validTypes.Contains(dto.Type))
                throw new BadRequestException("Yanlış əməliyyat növü.");

            var plan = Plans.FirstOrDefault(p => p.Id == dto.Plan)
                ?? throw new BadRequestException("Yanlış plan seçimi.");

            // Ödəniş prosesi (mock — real Stripe inteqrasiyası buraya gəlir)
            var paymentSuccess = await ProcessPaymentAsync(dto);

            var invoiceNumber = GenerateInvoiceNumber();

            var transaction = new Transaction
            {
                UserId = userId,
                Type = dto.Type,
                Plan = dto.Plan,
                Amount = plan.Price,
                Currency = plan.Currency,
                Status = paymentSuccess ? "completed" : "failed",
                PaymentMethod = dto.PaymentMethod,
                PaymentReference = Guid.NewGuid().ToString("N")[..16].ToUpper(),
                InvoiceNumber = invoiceNumber,
                CreatedAt = DateTime.UtcNow
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            if (!paymentSuccess)
                throw new BadRequestException("Ödəniş uğursuz oldu. Kart məlumatlarını yoxlayın.");

            return MapToDto(transaction);
        }

        public Task<IEnumerable<PlanDto>> GetPlansAsync()
            => Task.FromResult<IEnumerable<PlanDto>>(Plans);

        public async Task<byte[]> GenerateInvoicePdfAsync(int id, int userId)
        {
            var transaction = await _db.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)
                ?? throw new NotFoundException("Əməliyyat tapılmadı.");

            // Sadə HTML-dən text invoice (real layihədə PDF kitabxanası istifadə et)
            var content = $"""
            ================================================
            JOBBOARD - İNVOYS
            ================================================
            İnvoyce №:    {transaction.InvoiceNumber}
            Tarix:        {transaction.CreatedAt:dd.MM.yyyy HH:mm}
            ------------------------------------------------
            Müştəri:      {transaction.User.FullName}
            Email:        {transaction.User.Email}
            ------------------------------------------------
            Xidmət:       {transaction.Type}
            Plan:         {transaction.Plan}
            Məbləğ:       {transaction.Amount} {transaction.Currency}
            Ödəniş üsulu: {transaction.PaymentMethod}
            Status:       {transaction.Status}
            Ref №:        {transaction.PaymentReference}
            ================================================
            Təşəkkür edirik!
            JobBoard | www.jobboard.az
            """;

            return Encoding.UTF8.GetBytes(content);
        }

        private static Task<bool> ProcessPaymentAsync(TransactionCreateDto dto)
        {
            // Mock payment — real layihədə Stripe SDK istifadə et:
            // var paymentIntent = await _stripeClient.PaymentIntents.CreateAsync(...)
            if (dto.CardDetails != null && dto.CardDetails.Number == "4000000000000002")
                return Task.FromResult(false); // Test declined card

            return Task.FromResult(true);
        }

        private static string GenerateInvoiceNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"INV-{date}-{random}";
        }

        private static TransactionDto MapToDto(Transaction t) => new()
        {
            Id = t.Id,
            Type = t.Type,
            Plan = t.Plan,
            Amount = t.Amount,
            Currency = t.Currency,
            Status = t.Status,
            PaymentMethod = t.PaymentMethod,
            InvoiceNumber = t.InvoiceNumber,
            CreatedAt = t.CreatedAt
        };
    }

}
