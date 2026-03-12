using ArtemisBank.Core.Application.DTOs.User;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface IUserService
    {
        // Authentication
        Task<string> AuthenticateAsync(string email, string password);
        Task<string> RefreshTokenAsync(string token);
        Task RevokeTokenAsync(string token);

        // Registration
        Task<bool> RegisterAsync(string firstName, string lastName, string email, string password, string role);
        Task<bool> ConfirmEmailAsync(string userId, string token);

        // Password
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

        // User management
        Task<UserDto> GetByIdAsync(string userId);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<bool> ActivateUserAsync(string userId);
        Task<bool> DeactivateUserAsync(string userId);
    }
}
