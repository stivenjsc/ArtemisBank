using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId);
    }
}
