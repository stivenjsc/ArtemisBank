using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Dashboard;
using ArtemisBank.Core.Application.ViewModels.Account;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Client))]
    public class ClientController( IDashboardService dashboardService, ISavingsAccountService savingsAccountService, ILoanService loanService,
        ILoanInstallmentService installmentService, ICreditCardService creditCardService, ICreditCardConsumptionService consumptionService) : Controller
    {
        #region private fields
        private readonly IDashboardService _dashboardService = dashboardService;
        private readonly ISavingsAccountService _savingsAccountService = savingsAccountService;
        private readonly ILoanService _loanService = loanService;
        private readonly ILoanInstallmentService _installmentService = installmentService;
        private readonly ICreditCardService _creditCardService = creditCardService;
        private readonly ICreditCardConsumptionService _consumptionService = consumptionService;
        #endregion

        private string GetClientId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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

        public async Task<IActionResult> AccountDetail(string accountNumber)
        {
            var clientId = GetClientId();
            var account = await _savingsAccountService.GetByAccountNumberAsync(accountNumber);

            if (account == null) return NotFound();
            if (account.UserId != clientId) return Forbid();

            var transactions = await _savingsAccountService.GetTransactionsAsync(accountNumber);

            var vm = new AccountDetailViewModel
            {
                Account = account,
                Transactions = transactions
            };

            return View(vm);
        }

        public async Task<IActionResult> LoanDetail(int loanId)
        {
            var clientId = GetClientId();
            var loan = await _loanService.GetByIdAsync(loanId);

            if (loan == null) return NotFound();

            if (loan.ClientId != clientId) return Forbid();

            var installments = await _installmentService.GetByLoanIdAsync(loanId);
            var pendingAmount = await _installmentService.GetPendingAmountByLoanIdAsync(loanId);
            var paidCount = await _installmentService.GetPaidCountAsync(loanId);

            var vm = new ClientLoanDetailViewModel
            {
                Loan = loan,
                Installments = installments,
                TotalPendingAmount = pendingAmount,
                PaidInstallments = paidCount,
                TotalInstallments = loan.TermInMonths
            };

            return View(vm);
        }

        public async Task<IActionResult> CardDetail(int cardId)
        {
            var clientId = GetClientId();
            var card = await _creditCardService.GetByIdAsync(cardId);

            if (card == null) return NotFound();

            if (card.ClientId != clientId) return Forbid();

            var consumptions = await _consumptionService.GetByCardIdAsync(cardId);

            var vm = new CardDetailViewModel
            {
                CreditCard = card,
                Consumptions = consumptions
            };

            return View(vm);
        }
    }
}
