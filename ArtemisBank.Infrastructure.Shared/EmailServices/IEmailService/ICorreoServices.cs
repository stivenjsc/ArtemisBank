using ArtemisBank.Core.Application.Interfaces.IServices;
namespace ArtemisBank.Infrastructure.Shared.EmailServices.IEmailService
{
    public interface ICorreoServices : IEmailServices
    {
        Task SendEmailAsync(EmailRequest request);
    }
}
