using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.ViewModels.Loan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        public IActionResult Assign()
        {
            return View(new AssignLoanViewModel());
        }

        [HttpPost]
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

                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View(vm);
            }
        }
    }
}
