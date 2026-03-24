using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Domain.Enums;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface IUserService
    {
        // Authentication
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);

        // Registration
        Task<bool> RegisterAsync(string firstName, string lastName, string cedula, string username, string email, string password, string role,decimal initialAmount = 0);
        Task<bool> RegisterCommerceUserAsync(string firstName, string lastName, string cedula, string username, string email, string password, int commerceId);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<bool> ActivateAccountAsync(string token);

        // Password
        Task<bool> GeneratePasswordResetTokenAsync(string username);
        Task<bool> ResetPasswordAsync(string username, string token, string newPassword);

        // User management
        Task<UserDto> GetByIdAsync(string userId);
        Task<PaginatedResult<UserDto>> GetAllAsync(int page, int pageSize = 20, UserRole? role = null);
        Task<PaginatedResult<UserDto>> GetCommerceUsersAsync(int page, int pageSize = 20);
        Task<int> GetInactiveClientsCountAsync();
        Task<int> GetActiveClientsCountAsync();
        Task<IEnumerable<UserDto>> GetActiveClientsWithoutLoanAsync(string? cedula = null);
        Task<IEnumerable<UserDto>> GetActiveClientsAsync(string? cedula = null);

        Task<bool> UpdateAsync(UpdateUserDto dto);
        Task<bool> ChangeStatusAsync(string adminId, string userId, bool isActive);
        Task LogoutAsync();
        Task<string?> GetActivationTokenAsync(string userId);
    }
}
