namespace ArtemisBank.Core.Application.DTOs.User
{
    public class ChangeUserStatusDto
    {
        public string AdminId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
