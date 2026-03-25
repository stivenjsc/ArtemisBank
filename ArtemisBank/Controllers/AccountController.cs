using ArtemisBank.Core.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    [Authorize]
    public class AccountController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService;

        public IActionResult Index()
        {
            return View();
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> Activate(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "The activation link is invalid.";
                return RedirectToAction("Index", "Login");
            }

            var result = await _userService.ActivateAccountAsync(token);

            TempData[result ? "Success" : "Error"] = result ? "Account activated successfully. You can now log in." 
                : "Invalid or expired activation link.";
            return RedirectToAction("Index", "Login");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
