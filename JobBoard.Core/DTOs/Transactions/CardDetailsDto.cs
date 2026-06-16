using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Transactions
{

    public class CardDetailsDto
    {
        public string Number { get; set; } = null!;
        public string Expiry { get; set; } = null!;
        public string Cvv { get; set; } = null!;
        public string HolderName { get; set; } = null!;
    }

    public class TransactionCreateDto
    {
        public string Type { get; set; } = null!;
        public string Plan { get; set; } = null!;
        public int? JobId { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public CardDetailsDto? CardDetails { get; set; }
    }

    public class TransactionFilterDto
    {
        public string? Type { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string Plan { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PlanDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public List<string> Features { get; set; } = [];
        public bool IsPopular { get; set; }
    }
}
