using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<IEnumerable<CreditCard>> GetByClientIdAsync(string clientId);
        Task<CreditCard?> GetByCardNumberAsync(string cardNumber);
    }
}
