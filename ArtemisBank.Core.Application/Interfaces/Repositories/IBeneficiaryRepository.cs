using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface IBeneficiaryRepository : IGenericRepository<Beneficiary>
    {
        Task<IEnumerable<Beneficiary>> GetByOwnerIdAsync(string ownerId);
    }
}
