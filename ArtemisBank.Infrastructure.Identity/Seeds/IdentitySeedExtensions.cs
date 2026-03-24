using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArtemisBank.Infrastructure.Identity.Seeds
{
    public static class IdentitySeedExtensions
    {
        public class IdentitySeed { }
        public static async Task SeedIdentityDataAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                await DefaultRoles.SeedAsync(roleManager);
                await DefaultUsers.SeedAsync(userManager);
            }
            catch (Exception ex)
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = services.GetRequiredService<ILogger<IdentitySeed>>();
                logger.LogError(ex, "An error occurred during Identity seeding.");
            }
        }
    }
}
