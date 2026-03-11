using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface ILoanInstallmentRepository : IGenericRepository<LoanInstallment>
    {
        Task<IEnumerable<LoanInstallment>> GetByLoanIdAsync(int loanId);
    }
}
