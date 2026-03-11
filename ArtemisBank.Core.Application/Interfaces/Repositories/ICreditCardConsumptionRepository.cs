using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface ICreditCardConsumptionRepository : IGenericRepository<CreditCardConsumption>
    {
        Task<IEnumerable<CreditCardConsumption>> GetByCreditCardIdAsync(int creditCardId);
    }
}
