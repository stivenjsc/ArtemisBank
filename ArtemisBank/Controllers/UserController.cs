using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.User;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class UserController(IUserService userService, ISavingsAccountService savingsAccountService) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly ISavingsAccountService _savingsAccountService = savingsAccountService;

        public async Task<IActionResult> Index(int page = 1, UserRole? role = null)
        {
            var result = await _userService.GetAllAsync(page, 20, role);
            ViewBag.CurrentRole = role;
            ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(result);
        }

        public IActionResult Create()
        {
            return View(new SaveUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return View(vm);
            }
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var registered = await _userService.RegisterAsync(
                vm.FirstName,
                vm.LastName,
                vm.Cedula,
                vm.Username,
                vm.Email,
                vm.Password,
                vm.Role.ToString(),
                adminId!,
                vm.Role == UserRole.Client ? vm.InitialAmount ?? 0 : 0
            );

            if (!registered)
            {
                vm.HasError = true;
                vm.Error = "The user could not be created. Please verify the information and try again.";
                return View(vm);
            }
            TempData["Success"] = "User created. Activation email sent.";
            ModelState.Clear();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Cedula = user.Cedula,
                Email = user.Email,
                Username = user.UserName,
                Role = user.Role
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var updateDto = new UpdateUserDto
            {
                Id = vm.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Username = vm.Username,
                Cedula = vm.Cedula,
                Email = vm.Email,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword
            };

            var updated = await _userService.UpdateAsync(updateDto);

            if (!updated)
            {
                vm.HasError = true;
                vm.Error = "The user cannot be updated.";
                return View(vm);
            }

            if (vm.Role == UserRole.Client && vm.AdditionalAmount.HasValue && vm.AdditionalAmount.Value > 0)
            {
                var primaryAccount = await _savingsAccountService.GetPrimaryAccountByClientIdAsync(vm.Id);
                if (primaryAccount != null)
                {
                    await _savingsAccountService.DepositAsync(primaryAccount.AccountNumber, vm.AdditionalAmount.Value);
                }
            }
            TempData["Success"] = "The user has been updated succesfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string userId, bool activate)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }
            if (adminId == userId)
            {
                TempData["Error"] = "You cannot modified your own status.";
                return RedirectToAction("Index");
            }

            var result = await _userService.ChangeStatusAsync(adminId, userId, activate);

            if (result)
            {
                TempData["Success"] = activate ? "User successfully activated." : "User successfully deactivated.";
            }
            else
            {
                TempData["Error"] = "Could not change the user status.";
            }
            return RedirectToAction("Index");
        }
    }
}
