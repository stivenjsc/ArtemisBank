using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public AdminController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _dashboardService.GetAdminDashboardAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalTransactions = data.TotalTransactions,
                TotalActiveTransactions = data.TotalActiveTransactions,
                TotalInactiveTransactions = data.TotalInactiveTransactions,
                TotalDailyPayments = data.TotalDailyPayments,
                TotalAssignedProducts = data.TotalAssignedProducts,
                TotalActiveClients = data.TotalActiveClients,
                TotalInactiveClients = data.TotalInactiveClients
            };

            return View(vm);
        }
    }
}
