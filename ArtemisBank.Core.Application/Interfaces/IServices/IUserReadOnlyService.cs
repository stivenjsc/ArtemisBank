using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Domain.Enums;
namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface IUserReadOnlyService
    {
        // User management
        Task<UserDto> GetByIdAsync(string userId);
        Task<string?> GetActivationTokenAsync(string userId);

        Task<IEnumerable<UserDto>> GetActiveClientsAsync(string? cedula = null);

        Task<int> GetInactiveClientsCountAsync();
        Task<int> GetActiveClientsCountAsync();

        Task<PaginatedResult<UserDto>> GetAllAsync(int page, int pageSize = 20, UserRole? role = null);
        Task<PaginatedResult<UserDto>> GetCommerceUsersAsync(int page, int pageSize = 20);
    }
}