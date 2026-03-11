using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Domain.Interfaces
{
    public interface ICreditCardConsumptionRepository : IGenericRepository<CreditCardConsumption>
    {
        Task<IEnumerable<CreditCardConsumption>> GetByCardIdAsync(int creditCardId);
    }
}
