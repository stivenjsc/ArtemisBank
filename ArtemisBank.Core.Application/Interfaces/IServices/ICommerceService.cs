using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.Commerce;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ICommerceService
    {
        Task<CommerceDto> GetByIdAsync(int id);
        Task<IEnumerable<CommerceDto>> GetAllAsync();
        Task<PaginatedResult<CommerceDto>> GetAllPagedAsync(int page, int pageSize = 20);
        Task AddAsync(CommerceDto dto);
        Task UpdateAsync(CommerceDto dto);
        Task ChangeStatusAsync(int id, bool isActive);
        Task DeleteAsync(int id);
        Task<bool> CommerceHasActiveUserAsync(int commerceId);
    }
}
