using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.ViewModels.User;
using Microsoft.AspNetCore.Authentication;
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
                return User.IsInRole("Admin")
                    ? RedirectToAction("Index", "Admin")
                    : RedirectToAction("Index", "Client");
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

            if (result.Role == Core.Domain.Enums.UserRole.Admin)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Client");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
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

            // The service handles sending the reset email
            vm.HasError = false;
            vm.Error = null;
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
