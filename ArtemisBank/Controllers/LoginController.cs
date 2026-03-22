using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.User;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    public class LoginController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService;

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(nameof(UserRole.Admin)))
                    return RedirectToAction("Dashboard", "Admin");

                if (User.IsInRole(nameof(UserRole.Cashier)))
                    return RedirectToAction("Index", "Cashier");

                return RedirectToAction("Index", "Client");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _userService.AuthenticateAsync(vm.Username, vm.Password);

            if (!result.Success)
            {
                vm.HasError = true;
                vm.Error = result.Error ?? "Invalid Credential.";
                return View(vm);
            }

            return result.Role switch
            {
                UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                UserRole.Cashier => RedirectToAction("Index", "Cashier"),
                _ => RedirectToAction("Index", "Client")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("Index");
        }

        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var result = await _userService.GeneratePasswordResetTokenAsync(vm.Username);

            if (!result)
            {
                vm.HasError = true;
                vm.Error = "No user with that name was found.";
                return View(vm);
            }
            TempData["Success"] = "A reset link has been sent to your email.";
            return View("ForgotPasswordConfirmation");
        }

        public IActionResult ResetPassword(string username, string token)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
                return RedirectToAction("Index");

            return View(new ResetPasswordViewModel
            {
                Username = username,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _userService.ResetPasswordAsync(vm.Username, vm.Token, vm.NewPassword);

            if (!result)
            {
                vm.HasError = true;
                vm.Error = "The password could not be reset. The link may have expired.";
                return View(vm);
            }

            return RedirectToAction("Index");
        }
    }
}
