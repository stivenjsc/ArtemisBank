using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(IDashboardService dashboardService) : Controller
    {
        private readonly IDashboardService _dashboardService = dashboardService;

        public async Task<IActionResult> Dashboard()
        {
            var data = await _dashboardService.GetAdminDashboardAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalTransactions = data.TotalTransactions,
                TodayTransactions = data.TodayTransactions,
                TotalInactiveTransactions = data.TotalInactiveTransactions,
                TotalDailyPayments = data.TotalDailyPayments,
                TotalAssignedProducts = data.TotalAssignedProducts,
                TotalActiveClients = data.TotalActiveClients,
                TotalProducts = data.TotalProducts,
                ActiveLoans = data.ActiveLoans,
                ActiveCreditCards = data.ActiveCreditCards,
                TotalSavingsAccounts = data.TotalSavingsAccounts,
                AverageDebt = data.AverageDebt,
                TotalInactiveClients = data.TotalInactiveClients
            };

            return View(vm);
        }
    }
}
