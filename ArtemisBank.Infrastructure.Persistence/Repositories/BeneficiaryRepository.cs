using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces;
using ArtemisBank.Infrastructure.Persistence.Context;
using ArtemisBank.Infrastructure.Persistence.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class BeneficiaryRepository(ArtemisBankDbContext context) : GenericRepository<Beneficiary>(context), IBeneficiaryRepository
    {
        public async Task<bool> BeneficiaryExistForOwnerAsync(string ownerId, string accountNumber)
        {
            return await _dbSet.AnyAsync(b => b.OwnerId == ownerId && b.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Beneficiary>> GetByOwnerAccountIdAsync(string userId)
        {
            return await _dbSet.Where(b => b.OwnerId == userId).OrderBy(b => b.FirstName).ToListAsync();
        }
    }
}
