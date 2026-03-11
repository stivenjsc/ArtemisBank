namespace ArtemisBank.Infrastructure.Shared.EmailServices.IEmailServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailRequest request);
    }
}
