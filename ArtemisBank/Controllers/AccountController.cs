using ArtemisBank.Core.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    public class AccountController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService;

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Activate(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "The activation link is invalid.";
                return RedirectToAction("Index", "Login");
            }

            var result = await _userService.ActivateAccountAsync(token);

            if (!result)
            {
                TempData["Error"] = "The activation link is invalid or has already been used.";
                return RedirectToAction("Index", "Login");
            }

            TempData["Success"] = "Your account has been activated. You can now log in.";
            return RedirectToAction("Index", "Login");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
