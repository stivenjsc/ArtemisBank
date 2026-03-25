using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels.CreditCard;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Client)}")]
    public class CreditCardController( ICreditCardService creditCardService, ISavingsAccountService savingsAccountService,
        ICreditCardConsumptionService consumptionService, ILoanService loan, IUserService user) : Controller
    {
        private readonly ICreditCardService _creditCardService = creditCardService;
        private readonly ISavingsAccountService _savingsAccountService = savingsAccountService;
        private readonly ICreditCardConsumptionService _consumptionService = consumptionService;
        private readonly ILoanService _loanService = loan;
        private readonly IUserService _userService = user;

        #region Admin - List

        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Index(int page = 1, CardStatus? status = null, string? cedula = null)
        {
            var result = await _creditCardService.GetAllPagedAsync(page, 20, status, cedula);
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentCedula = cedula;
            return View(result);
        }

        #endregion

        #region Admin - Detail with Consumptions

        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Detail(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return NotFound();

            var consumptions = await _consumptionService.GetByCardIdAsync(id);

            var vm = new CreditCardDetailViewModel
            {
                CreditCard = card,
                Consumptions = consumptions
            };

            return View(vm);
        }

        #endregion

        #region Admin - Assign Credit Card

        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Assign()
        {
            var averageDebt = await _loanService.GetAverageDebtAsync();
            var clients = await _userService.GetActiveClientsAsync(null);

            var vm = new AssignCreditCardViewModel
            {
                AverageDebt = averageDebt,
                Clients = clients,
                CreditLimit = 0
            };

            return View(vm);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AverageDebt = await _loanService.GetAverageDebtAsync();
                vm.Clients = await _userService.GetActiveClientsAsync(null);
                return View(vm);
            }

            try
            {
                await _creditCardService.AssignAsync(new AssignCreditCardDto
                {
                    ClientId = vm.ClientId,
                    CreditLimit = vm.CreditLimit
                });
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.AverageDebt = await _loanService.GetAverageDebtAsync();
                vm.Clients = await _userService.GetActiveClientsAsync(null);
                return View(vm);
            }
        }

        #endregion

        #region Admin - Edit Credit Limit

        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> EditLimit(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return NotFound();

            var vm = new EditCreditCardLimitViewModel
            {
                CardId = card.Id,
                CardNumber = card.CardNumber,
                CurrentCreditLimit = card.CreditLimit,
                NewCreditLimit = card.CreditLimit,
                AmountOwed = card.AmountOwed
            };

            return View(vm);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLimit(EditCreditCardLimitViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                await _creditCardService.UpdateLimitAsync(vm.CardId, vm.NewCreditLimit);
                TempData["Success"] = "Limit updated. Client notified by email.";
                return RedirectToAction("Detail", new { id = vm.CardId });
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View(vm);
            }
        }

        #endregion

        #region Admin - Cancel Card

        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Cancel(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return NotFound();

            var vm = new CancelCreditCardViewModel
            {
                CardId = card.Id,
                LastFourDigits = card.CardNumber.Substring(12),
                AmountOwed = card.AmountOwed
            };

            return View(vm);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelCreditCardViewModel vm)
        {
            try
            {
                await _creditCardService.CancelAsync(vm.CardId);

                TempData["Success"] = "Tarjeta cancelada correctamente.";
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

        #region Client - Cash Advance

        [Authorize(Roles = nameof(UserRole.Client))]
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

        [Authorize(Roles = nameof(UserRole.Client))]
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                TempData["Success"] = "Cash advance made. Notified by mail.";
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
