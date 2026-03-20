using ArtemisBank.Core.Domain.Enums;

namespace ArtemisBank.Core.Application.DTOs.Loan
{
    public class LoanDto
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal AnualInterestRate { get; set; }
        public int TermInMonths { get; set; }
        public LoanStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ClientFullName { get; set; } = string.Empty;
    }
}
