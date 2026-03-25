using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Client;
using ArtemisBank.Core.Application.ViewModels.Loan;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class LoanController(ILoanService loanService, ILoanInstallmentService installmentService, IUserReadOnlyService userService) : Controller
    {
        private readonly ILoanService _loanService = loanService;
        private readonly ILoanInstallmentService _installmentService = installmentService;
        private readonly IUserReadOnlyService _userService = userService;

        #region List

        public async Task<IActionResult> Index(int page = 1, LoanStatus? status = null, string? cedula = null)
        {
            var result = await _loanService.GetAllPagedAsync(page, 20, status, cedula);
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentCedula = cedula;
            return View(result);
        }

        #endregion

        #region Detail with Amortization Table

        public async Task<IActionResult> Detail(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return NotFound();

            var installments = await _installmentService.GetByLoanIdAsync(id);
            var pendingAmount = await _installmentService.GetPendingAmountByLoanIdAsync(id);
            var paidCount = await _installmentService.GetPaidCountAsync(id);

            var vm = new LoanDetailViewModel
            {
                Loan = loan,
                Installments = installments,
                TotalPendingAmount = pendingAmount,
                PaidInstallments = paidCount,
                TotalInstallments = loan.TermInMonths
            };

            return View(vm);
        }

        #endregion

        #region Step 1 - Select Client

        public async Task<IActionResult> SelectClient(string? cedula = null)
        {
            var averageDebt = await _loanService.GetAverageDebtAsync();
            var clients = await _loanService.GetActiveClientsWithoutLoanAsync(cedula);

            var vm = new SelectClientViewModel
            {
                AverageDebt = averageDebt,
                Clients = clients,
                CurrentCedula = cedula
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectClient(SelectClientViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.SelectedClientId))
            {
                vm.Clients = await _loanService.GetActiveClientsWithoutLoanAsync(vm.CurrentCedula);
                ViewBag.Error = "Please select a client.";
                return View(vm);
            }

            var client = await _userService.GetByIdAsync(vm.SelectedClientId);
            if (client == null)
            {
                return NotFound();
            }
            var averageDebt = await _loanService.GetAverageDebtAsync();
            var currentDebt = await _loanService.GetTotalDebtByClientIdAsync(vm.SelectedClientId);

            var assignVm = new AssignLoanViewModel
            {
                ClientId = vm.SelectedClientId,
                ClientName = $"{client.FirstName} {client.LastName}",
                AverageDebt = averageDebt,
                CurrentDebt = currentDebt,
                IsHighRisk = currentDebt > averageDebt
            };

            return View("Assign", assignVm);
        }

        #endregion

        #region Step 2 - Assign Loan

        public async Task<IActionResult> Assign(string clientId, string clientName, bool isHighRisk = false)
        {
            clientId = TempData["SelectedClientId"]?.ToString()!;

            if (string.IsNullOrEmpty(clientId)) return RedirectToAction("SelectClient");

            TempData.Keep("SelectedClientId");

            var client = await _userService.GetByIdAsync(clientId);
            if (client == null) return NotFound();

            var averageDebt = await _loanService.GetAverageDebtAsync();
            var currentDebt = await _loanService.GetTotalDebtByClientIdAsync(clientId);

            var vm = new AssignLoanViewModel
            {
                ClientId = clientId,
                ClientName = clientName,
                AverageDebt = averageDebt,
                CurrentDebt = currentDebt,
                IsHighRisk = currentDebt > averageDebt
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignLoanViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var riskInfo = await _loanService.EvaluateRiskAsync(vm.ClientId, vm.Amount, vm.AnnualInterestRate, vm.TermInMonths);

            if (riskInfo.IsHighRisk && !vm.RiskConfirmed)
            {
                vm.IsHighRisk = true;
                vm.AverageDebt = riskInfo.AverageDebt;
                vm.CurrentDebt = riskInfo.CurrentDebt;
                vm.RiskMessage = riskInfo.CurrentDebt > riskInfo.AverageDebt ? "High Risk: The client's current debt already exceeds the bank average." 
                    : "High Risk: This new loan will push the client's total debt above the bank average.";

                return View(vm);
            }
            try
            {
                await _loanService.AssignAsync(new AssignLoanDto
                {
                    ClientId = vm.ClientId,
                    Amount = vm.Amount,
                    AnnualInterestRate = vm.AnnualInterestRate,
                    TermInMonths = vm.TermInMonths
                });

                TempData["Success"] = "Loan has been successfully assigned.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.AverageDebt = riskInfo.AverageDebt;
                vm.CurrentDebt = riskInfo.CurrentDebt;
                return View(vm);
            }
        }

        #endregion

        #region Edit Interest Rate

        public async Task<IActionResult> EditRate(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return NotFound();

            var vm = new EditLoanRateViewModel
            {
                LoanId = loan.Id,
                LoanNumber = loan.LoanNumber,
                CurrentAnnualInterestRate = loan.AnnualInterestRate,
                NewAnnualInterestRate = loan.AnnualInterestRate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRate(EditLoanRateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                await _loanService.UpdateInterestRateAsync(vm.LoanId, vm.NewAnnualInterestRate);
                TempData["Success"] = "Rate updated. Future installments were recalculated and the client was notified.";
                return RedirectToAction("Detail", new { id = vm.LoanId });
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View(vm);
            }
        }

        #endregion
    }
}
