using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Application.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISavingsAccountService _savingsAccountService;

        public UserController(IUserService userService, ISavingsAccountService savingsAccountService)
        {
            _userService = userService;
            _savingsAccountService = savingsAccountService;
        }

        public async Task<IActionResult> Index(int page = 1, UserRole? role = null)
        {
            var result = await _userService.GetAllAsync(page, 20, role);
            ViewBag.CurrentRole = role;
            return View(result);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SaveUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var registered = await _userService.RegisterAsync(
                vm.FirstName,
                vm.LastName,
                vm.Cedula,
                vm.Username,
                vm.Email,
                vm.Password,
                vm.Role.ToString()
            );

            if (!registered)
            {
                vm.HasError = true;
                vm.Error = "No se pudo crear el usuario. Verifique los datos e intente de nuevo.";
                return View(vm);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
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
                Username = user.UserName
            };

            return View(vm);
        }

        [HttpPost]
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
                Cedula = vm.Cedula,
                Email = vm.Email,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword
            };

            var updated = await _userService.UpdateAsync(updateDto);

            if (!updated)
            {
                vm.HasError = true;
                vm.Error = "No se pudo actualizar el usuario.";
                return View(vm);
            }

            if (vm.AdditionalAmount.HasValue && vm.AdditionalAmount.Value > 0)
            {
                var primaryAccount = await _savingsAccountService.GetPrimaryAccountByClientIdAsync(vm.Id);
                if (primaryAccount != null)
                {
                    await _savingsAccountService.DepositAsync(primaryAccount.AccountNumber, vm.AdditionalAmount.Value);
                }
            }

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

            await _userService.ChangeStatusAsync(adminId, userId, activate);
            return RedirectToAction("Index");
        }
    }
}
