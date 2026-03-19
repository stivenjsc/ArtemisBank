using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.ViewModels.CreditCard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly ISavingsAccountService _savingsAccountService;

        public CreditCardController(ICreditCardService creditCardService, ISavingsAccountService savingsAccountService)
        {
            _creditCardService = creditCardService;
            _savingsAccountService = savingsAccountService;
        }

        #region Admin - Assign Credit Card

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Assign()
        {
            return View(new AssignCreditCardViewModel());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Assign(AssignCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                await _creditCardService.AssignAsync(new AssignCreditCardDto
                {
                    ClientId = vm.ClientId,
                    CreditLimit = vm.CreditLimit
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

        #endregion

        #region Client - Cash Advance

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> CashAdvance()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var vm = new CashAdvanceViewModel
            {
                UserCreditCards = await _creditCardService.GetActiveByClientIdAsync(clientId),
                UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId)
            };
            return View(vm);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CashAdvance(CashAdvanceViewModel vm)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                vm.UserCreditCards = await _creditCardService.GetActiveByClientIdAsync(clientId);
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                return View(vm);
            }

            try
            {
                await _creditCardService.CashAdvanceAsync(new CashAdvanceDto
                {
                    CreditCardId = vm.CreditCardId,
                    SavingsAccountId = vm.SavingsAccountId,
                    Amount = vm.Amount
                });

                return RedirectToAction("Index", "Client");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.UserCreditCards = await _creditCardService.GetActiveByClientIdAsync(clientId);
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                return View(vm);
            }
        }

        #endregion
    }
}
