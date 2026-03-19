using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            var defaultAdmin = new ApplicationUser
            {
                UserName = "adminuser",
                Email = "admin@artemisbank.com",
                FirstName = "Admin",
                LastName = "ArtemisBank",
                Cedula = "00000000000",
                EmailConfirmed = true,
                IsActive = true
            };

            var user = await userManager.FindByEmailAsync(defaultAdmin.Email);

            if (user == null)
            {
                await userManager.CreateAsync(defaultAdmin, "Admin123!");
                await userManager.AddToRoleAsync(defaultAdmin, UserRole.Admin.ToString());
            }
        }
    }
}
