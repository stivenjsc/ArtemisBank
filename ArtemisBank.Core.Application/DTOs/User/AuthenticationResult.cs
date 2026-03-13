using ArtemisBank.Core.Domain.Enums;

namespace ArtemisBank.Core.Application.DTOs.User
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? Error { get; set; }
    }
}
