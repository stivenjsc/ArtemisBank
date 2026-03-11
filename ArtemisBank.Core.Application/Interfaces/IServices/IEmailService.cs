namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
