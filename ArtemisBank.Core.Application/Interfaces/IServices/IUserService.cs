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
        Task<bool> RegisterAsync(string firstName, string lastName, string cedula, string username, string email, string password, string role);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<bool> ActivateAccountAsync(string token);

        // Password
        Task<bool> GeneratePasswordResetTokenAsync(string username);
        Task<bool> ResetPasswordAsync(string username, string token, string newPassword);

        // User management
        Task<UserDto> GetByIdAsync(string userId);
        Task<PaginatedResult<UserDto>> GetAllAsync(int page, int pageSize = 20, UserRole? role = null);
        Task<bool> UpdateAsync(UpdateUserDto dto);
        Task<bool> ChangeStatusAsync(string adminId, string userId, bool isActive);
        Task LogoutAsync();
        Task<string?> GetActivationTokenAsync(string userId);
    }
}
