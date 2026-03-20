namespace ArtemisBank.Core.Application.DTOs.Account
{
    public class AssignSavingsAccountDto
    {
        public string ClientId { get; set; } = string.Empty;
        public int InitialBalance { get; set; }
    }
}
