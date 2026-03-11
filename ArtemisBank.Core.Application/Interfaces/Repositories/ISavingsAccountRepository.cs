using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
    {
        Task<IEnumerable<SavingsAccount>> GetByUserIdAsync(string userId);
        Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber);
    }
}
