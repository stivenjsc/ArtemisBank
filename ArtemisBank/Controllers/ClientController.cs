using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public ClientController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var data = await _dashboardService.GetClientDashboardAsync(clientId);

            var vm = new ClientDashboardViewModel
            {
                TotalSavingsAccounts = data.TotalSavingsAccounts,
                TotalCreditCards = data.TotalCreditCards,
                TotalLoans = data.TotalLoans,
                SavingsAccounts = data.SavingsAccounts,
                CreditCards = data.CreditCards,
                Loans = data.Loans
            };

            return View(vm);
        }
    }
}
