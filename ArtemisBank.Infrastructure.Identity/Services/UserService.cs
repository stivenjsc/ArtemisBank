using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return new AuthenticationResult { Success = false, Error = "User not found." };
            }

            if (!user.IsActive)
            {
                return new AuthenticationResult { Success = false, Error = "The account is disabled." };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthenticationResult { Success = false, Error = "The email is not confirmed yet." };
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return new AuthenticationResult { Success = false, Error = "Invalid credentials." };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = Enum.Parse<UserRole>(roles.First());

            return new AuthenticationResult
            {
                Success = true,
                UserId = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Role = role
            };
        }

        public async Task<bool> RegisterAsync(string firstName, string lastName, string username, string email, string password, string role)
        {
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null) return false;

            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null) return false;

            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = username,
                Email = email,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded) return false;

            await _userManager.AddToRoleAsync(user, role);
            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(string username, string token, string newPassword)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task<UserDto> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null!;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Cedula = user.Cedula,
                Email = user.Email!,
                Role = Enum.Parse<UserRole>(roles.FirstOrDefault() ?? nameof(UserRole.Client)),
                IsActive = user.IsActive
            };
        }

        public async Task<PaginatedResult<UserDto>> GetAllAsync(int page, int pageSize = 20, UserRole? role = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (role.HasValue)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Value.ToString());
                var userIds = usersInRole.Select(u => u.Id).ToHashSet();
                query = query.Where(u => userIds.Contains(u.Id));
            }

            var totalCount = query.Count();
            var users = query
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Cedula = user.Cedula,
                    Email = user.Email!,
                    Role = Enum.Parse<UserRole>(roles.FirstOrDefault() ?? nameof(UserRole.Client)),
                    IsActive = user.IsActive
                });
            }

            return new PaginatedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ChangeStatusAsync(string adminId, string userId, bool isActive)
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null) return false;

            var adminRoles = await _userManager.GetRolesAsync(admin);
            if (!adminRoles.Contains(UserRole.Admin.ToString())) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
