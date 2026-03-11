using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Domain.Interfaces
{
    public interface IBeneficiaryRepository : IGenericRepository<Beneficiary>
    {
        // get all beneficiaries for a specific user
        Task<IEnumerable<Beneficiary>> GetByOwnerAccountIdAsync(string userId);
        Task<bool> BeneficiaryExistForOwnerAsync(string ownerId, string accountNumber);
    }
}
