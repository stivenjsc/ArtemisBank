using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces;
using ArtemisBank.Infrastructure.Identity.Context;
using ArtemisBank.Infrastructure.Persistence.Context;
using ArtemisBank.Infrastructure.Persistence.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class CommerceRepository(ArtemisBankDbContext context, IdentityContext identity) : GenericRepository<Commerce>(context), ICommerceRepository
    {
        private readonly IdentityContext _identity = identity;
        public async Task<bool> CommerceHasActiveUserAsync(int commerceId)
        {
            return await _identity.Users.AnyAsync(u => u.CommerceId == commerceId && u.IsActive);
        }

        public async Task<IEnumerable<Commerce>> GetAllPagedAsync(int? page = null, int? pageSize = null)
        {
            var query = _dbSet.AsNoTracking().Where(c => c.IsActive).OrderByDescending(c => c.CreatedAt);

            if (page.HasValue && pageSize.HasValue)
            {
                return await query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToListAsync();
            }
            return await query.ToListAsync();
        }

        public async Task<Commerce> GetByIdWithUserAsync(int commerceId)
        {
            throw new NotImplementedException();
        }
    }
}
