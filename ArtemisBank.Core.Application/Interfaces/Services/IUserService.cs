namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<bool> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(string firstName, string lastName, string email, string password, string role);
        Task<bool> ActivateUserAsync(string userId);
        Task<bool> DeactivateUserAsync(string userId);
    }
}
