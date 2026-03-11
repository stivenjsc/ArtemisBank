using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Domain.Interfaces
{
    public interface ILoanInstallmentRepository : IGenericRepository<LoanInstallment>
    {
        Task<int> GetPaidInstallmentsCountAsync(int loanId);
        Task<decimal> GetPendingAmountByLoanIdAsync(int loanId);
        Task<IEnumerable<LoanInstallment>> GetByLoanIdAsync(int loanId);
        // get all installments that are overdue and not fully paid
        Task<IEnumerable<LoanInstallment>> GetOverdueInstallmentsAsync();
        Task<LoanInstallment?> GetFirstPendingInstallmentAsync(int loanId);
        Task<IEnumerable<LoanInstallment>> GetFutureUnpaidInstallmentsAsync(int loanId);
    }
}
