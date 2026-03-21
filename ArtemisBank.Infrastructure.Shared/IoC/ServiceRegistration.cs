using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Infrastructure.Shared.EmailServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBank.Infrastructure.Shared.IoC
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailServices, EmailService>();
        }
    }
}
