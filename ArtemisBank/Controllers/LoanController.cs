using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Loan;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _installmentService;
        private readonly IUserService _userService;

        public LoanController(ILoanService loanService, ILoanInstallmentService installmentService, IUserService userService)
        {
            _loanService = loanService;
            _installmentService = installmentService;
            _userService = userService;
        }

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

        [HttpGet]
        public IActionResult SelectClient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectClient(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                ViewBag.Error = "Debe ingresar el ID del cliente.";
                return View();
            }

            var client = await _userService.GetByIdAsync(clientId);
            if (client == null)
            {
                ViewBag.Error = "Cliente no encontrado.";
                return View();
            }

            var hasActiveLoan = await _loanService.ClientHasActiveLoanAsync(clientId);

            var vm = new AssignLoanViewModel
            {
                ClientId = clientId,
                ClientName = $"{client.FirstName} {client.LastName}",
                IsHighRisk = hasActiveLoan
            };

            return View("Assign", vm);
        }

        #endregion

        #region Step 2 - Assign Loan

        [HttpGet]
        public IActionResult Assign(string clientId, string clientName, bool isHighRisk = false)
        {
            var vm = new AssignLoanViewModel
            {
                ClientId = clientId,
                ClientName = clientName,
                IsHighRisk = isHighRisk
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignLoanViewModel vm)
        {
            if (!ModelState.IsValid)
            {
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

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View(vm);
            }
        }

        #endregion

        #region Edit Interest Rate

        [HttpGet]
        public async Task<IActionResult> EditRate(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return NotFound();

            var vm = new EditLoanRateViewModel
            {
                LoanId = loan.Id,
                LoanNumber = loan.LoanNumber,
                CurrentAnnualInterestRate = loan.AnualInterestRate,
                NewAnnualInterestRate = loan.AnualInterestRate
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
