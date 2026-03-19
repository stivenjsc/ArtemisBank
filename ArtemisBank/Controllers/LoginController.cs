using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.User;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(nameof(UserRole.Admin)))
                    return RedirectToAction("Index", "Admin");

                if (User.IsInRole(nameof(UserRole.Cashier)))
                    return RedirectToAction("Index", "Cashier");

                return RedirectToAction("Index", "Client");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
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
                vm.Error = result.Error ?? "Credenciales inválidas.";
                return View(vm);
            }

            return result.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string username, string token)
        {
            return View(new ResetPasswordViewModel { Username = username, Token = token });
        }

        [HttpPost]
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
                vm.Error = "No se pudo restablecer la contraseña.";
                return View(vm);
            }

            return RedirectToAction("Index");
        }
    }
}
