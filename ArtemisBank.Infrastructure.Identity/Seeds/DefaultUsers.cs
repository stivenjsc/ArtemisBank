using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, int? defaultCommerceId = null)
        {
            await SeedUserAsync(userManager, new ApplicationUser
            {
                UserName = "AdminUser",
                Email = "admin@artemisbank.com",
                FirstName = "Admin",
                LastName = "ArtemisBank",
                Cedula = "00000000001",
                EmailConfirmed = true,
                IsActive = true,
                Role = UserRole.Admin
            }, "Admin123!", UserRole.Admin);

            await SeedUserAsync(userManager, new ApplicationUser
            {
                UserName = "CashierUser",
                Email = "cashier@artemisbank.com",
                FirstName = "Cashier",
                LastName = "ArtemisBank",
                Cedula = "00000000002",
                EmailConfirmed = true,
                IsActive = true,
                Role = UserRole.Cashier
            }, "Cashier123!", UserRole.Cashier);

            await SeedUserAsync(userManager, new ApplicationUser
            {
                UserName = "ClientUser",
                Email = "client@artemisbank.com",
                FirstName = "Client",
                LastName = "ArtemisBank",
                Cedula = "00000000003",
                EmailConfirmed = true,
                IsActive = true,
                Role = UserRole.Client
            }, "Client123!", UserRole.Client);

            if (defaultCommerceId.HasValue)
            {
                await SeedUserAsync(userManager, new ApplicationUser
                {
                    UserName = "CommerceUser",
                    Email = "commerce@artemisbank.com",
                    FirstName = "Commerce",
                    LastName = "ArtemisBank",
                    Cedula = "00000000004",
                    EmailConfirmed = true,
                    IsActive = true,
                    Role = UserRole.Commerce,
                    CommerceId = defaultCommerceId.Value
                }, "Commerce123!", UserRole.Commerce);
            }
        }

        private static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string password, UserRole role)
        {
            if (await userManager.FindByEmailAsync(user.Email!) == null)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role.ToString());
                }
            }
        }
    }
}