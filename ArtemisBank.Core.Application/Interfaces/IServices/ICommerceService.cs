using ArtemisBank.Core.Application.DTOs.Commerce;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ICommerceService
    {
        Task<CommerceDto> GetByIdAsync(int id);
        Task<IEnumerable<CommerceDto>> GetAllAsync();
        Task AddAsync(CommerceDto dto);
        Task UpdateAsync(CommerceDto dto);
        Task DeleteAsync(int id);
    }
}
