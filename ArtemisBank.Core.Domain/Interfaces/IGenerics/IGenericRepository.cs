namespace ArtemisBank.Core.Domain.Interfaces.IGenerics
{
    public interface IGenericRepository<Entity> where Entity : class
    {
        Task<Entity> GetByIdAsync(int id);
        Task<IEnumerable<Entity>> GetAllAsync();
        Task AddAsync(Entity entity);
        Task DeleteAsync(int id);
        Task UpdateAsync(Entity entity);
    }
}