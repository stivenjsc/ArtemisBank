using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Domain.Interfaces
{
    public interface ICommerceRepository : IGenericRepository<Commerce>
    {
        Task<Commerce> GetByIdWithUserAsync(int commerceId);
        Task<IEnumerable<Commerce>> GetAllPagedAsync(int? page = null, int? pageSize = null);
        Task<bool> CommerceHasActiveUserAsync(int commerceId);
    }
}