using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Application.Interfaces.Repositories
{
    public interface ICommerceRepository : IGenericRepository<Commerce>
    {
        Task<Commerce?> GetByNameAsync(string name);
    }
}
