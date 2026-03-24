using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.DTOs.LoanInstallment;

namespace ArtemisBank.Core.Application.ViewModels.Account
{
    public class ClientLoanDetailViewModel
    {
        public IEnumerable<LoanInstallmentDto> Installments { get; set; } = new List<LoanInstallmentDto>();
        public decimal TotalPendingAmount { get; set; }
        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public LoanDto Loan { get; set; } = new LoanDto();
    }
}
