using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Plan { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "pending";
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}
