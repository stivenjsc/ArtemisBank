using ArtemisBank.Core.Application.DTOs.Account;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Account;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class SavingsAccountController(
        ISavingsAccountService savingsAccountService,
        IUserReadOnlyService userService) : Controller
    {
        private readonly ISavingsAccountService _savingsAccountService = savingsAccountService;
        private readonly IUserReadOnlyService _userService = userService;
        
        #region List

        public async Task<IActionResult> Index( int page = 1, AccountStatus? status = null, AccountType? type = null, string? cedula = null)
        {
            var result = await _savingsAccountService.GetAllPagedAsync(page, 20, status, type, cedula);
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentType = type;
            ViewBag.CurrentCedula = cedula;
            return View(result);
        }

        #endregion

        #region Details - transactions

        public async Task<IActionResult> Detail(string accountNumber)
        {
            var account = await _savingsAccountService.GetByAccountNumberAsync(accountNumber);
            if (account == null) return NotFound();

            var transactions = await _savingsAccountService.GetTransactionsAsync(accountNumber);

            var vm = new SavingsAccountDetailViewModel
            {
                Account = account,
                Transactions = transactions
            };

            return View(vm);
        }

        #endregion

        #region 1 - select client

        public async Task<IActionResult> SelectClient(string? cedula = null)
        {
            var clients = await _userService.GetActiveClientsAsync(cedula);

            var vm = new SelectClientForAccountViewModel
            {
                Clients = clients,
                CurrentCedula = cedula
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectClient(SelectClientForAccountViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.SelectedClientId))
            {
                TempData["Error"] = "Debe seleccionar un cliente.";
                return RedirectToAction("SelectClient");
            }

            TempData["SelectedClientId"] = vm.SelectedClientId;
            return RedirectToAction("Assign");
        }

        #endregion

        #region 2 - Assign secundary account

        public async Task<IActionResult> Assign()
        {
            var clientId = TempData["SelectedClientId"]?.ToString();

            if (string.IsNullOrEmpty(clientId))
                return RedirectToAction("SelectClient");

            TempData.Keep("SelectedClientId");

            var client = await _userService.GetByIdAsync(clientId);
            if (client == null) return NotFound();

            var vm = new AssignSavingsAccountViewModel
            {
                ClientId = clientId,
                ClientName = $"{client.FirstName} {client.LastName}",
                InitialBalance = 0
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignSavingsAccountViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _savingsAccountService.AssignSecondaryAsync(new AssignSavingsAccountDto
                {
                    ClientId = vm.ClientId,
                    InitialBalance = vm.InitialBalance
                });

                TempData["Success"] = "Cuenta de ahorro secundaria asignada correctamente.";
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

        #region Cancel secondary account

        public async Task<IActionResult> Cancel(string accountNumber)
        {
            var account = await _savingsAccountService.GetByAccountNumberAsync(accountNumber);
            if (account == null) return NotFound();

            if (account.Type == AccountType.Primary)
            {
                TempData["Error"] = "Primary accounts cannot be cancelled.";
                return RedirectToAction("Index");
            }

            return View(new CancelSavingsAccountViewModel
            {
                AccountNumber = accountNumber,
                Balance = account.Balance
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelSavingsAccountViewModel vm)
        {
            try
            {
                await _savingsAccountService.CancelAsync(vm.AccountNumber);

                TempData["Success"] = "Account closed. The balance was transferred to the main account.";
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
    }
}
