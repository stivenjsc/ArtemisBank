using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces;
using ArtemisBank.Infrastructure.Persistence.Context;
using ArtemisBank.Infrastructure.Persistence.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class CreditCardConsumptionRepository(ArtemisBankDbContext context) : GenericRepository<CreditCardConsumption>(context), ICreditCardConsumptionRepository
    {
        public async Task<IEnumerable<CreditCardConsumption>> GetByCardIdAsync(int creditCardId)
        {
            return await _dbSet.Where(c => c.CreditCardId == creditCardId).OrderByDescending(c => c.TransactionDate).ToListAsync();
        }
    }
}
