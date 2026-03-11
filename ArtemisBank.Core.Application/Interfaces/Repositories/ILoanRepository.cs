using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<IEnumerable<Loan>> GetByUserIdAsync(string userId);
        Task<Loan?> GetByLoanNumberAsync(string loanNumber);
    }
}
