using ArtemisBank.Core.Application.DTOs.Account;
using ArtemisBank.Core.Application.DTOs.Cashier;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Cashier;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Cashier))]
    public class CashierController( ITransactionService transactionService, ISavingsAccountService savingsAccountService, ICreditCardService creditCardService,
        ILoanService loanService, IDashboardService dashboardService) : Controller
    {
        private readonly ITransactionService _transactionService = transactionService;
        private readonly ISavingsAccountService _savingsAccountService = savingsAccountService;
        private readonly ICreditCardService _creditCardService = creditCardService;
        private readonly ILoanService _loanService = loanService;
        private readonly IDashboardService _dashboardService = dashboardService;

        private string GetCashierId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        #region Dashboard cashier

        public async Task<IActionResult> Index()
        {
            var cashierId = GetCashierId();
            var data = await _dashboardService.GetCashierDashboardAsync(cashierId);

            var vm = new CashierDashboardViewModel
            {
                TodayTransactions = data.TodayTransactions,
                TodayPayments = data.TodayPayments,
                TodayDeposits = data.TodayDeposits,
                TodayWithdrawals = data.TodayWithdrawals
            };

            return View(vm);
        }

        #endregion

        #region Deposits

        public IActionResult Deposit()
        {
            return View(new CashierDepositViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(CashierDepositViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);

            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Error = "The account number entered is not valid.";
                return View(vm);
            }

            vm.AccountHolderName = account.OwnerFullName;
            return View("DepositConfirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepositConfirm(CashierDepositViewModel vm)
        {
            try
            {
                await _transactionService.DepositAsync(new CashierDepositDto
                {
                    AccountNumber = vm.AccountNumber,
                    Amount = vm.Amount
                });

                TempData["Success"] = "Deposit made. Customer notified by email.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View("DepositConfirm", vm);
            }
        }

        #endregion

        #region Withdrawal

        public IActionResult Withdrawal()
        {
            return View(new CashierWithdrawalViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdrawal(CashierWithdrawalViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);

            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Error = "The account number entered is not valid.";
                return View(vm);
            }

            if (account.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Error = "The amount exceeds the available balance in the account.";
                return View(vm);
            }

            vm.AccountHolderName = account.OwnerFullName;
            return View("WithdrawalConfirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawalConfirm(CashierWithdrawalViewModel vm)
        {
            try
            {
                await _transactionService.WithdrawAsync(new CashierWithdrawalDto
                {
                    AccountNumber = vm.AccountNumber,
                    Amount = vm.Amount
                });

                TempData["Success"] = "Withdrawal completed. Customer notified by email.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View("WithdrawalConfirm", vm);
            }
        }

        #endregion

        #region Credit card payment

        public IActionResult PayCreditCard()
        {
            return View(new CashierPayCreditCardViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayCreditCard(CashierPayCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.SourceAccountNumber);
            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Error = "The originating account number is invalid.";
                return View(vm);
            }

            var card = await _creditCardService.GetByCardNumberAsync(vm.CardNumber);
            if (card == null || card.Status != CardStatus.Active)
            {
                vm.HasError = true;
                vm.Error = "The card number entered is not valid.";
                return View(vm);
            }

            if (account.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Error = "The amount exceeds the available balance in the account.";
                return View(vm);
            }

            vm.CardHolderName = card.ClientFullName;
            return View("PayCreditCardConfirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayCreditCardConfirm(CashierPayCreditCardViewModel vm)
        {
            try
            {
                await _transactionService.CashierPayCreditCardAsync(new CashierPayCreditCardDto
                {
                    SourceAccountNumber = vm.SourceAccountNumber,
                    CardNumber = vm.CardNumber,
                    Amount = vm.Amount
                });

                TempData["Success"] = "Card payment made. Customer notified by email.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View("PayCreditCardConfirm", vm);
            }
        }

        #endregion

        #region Loan payment

        public IActionResult PayLoan()
        {
            return View(new CashierPayLoanViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayLoan(CashierPayLoanViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.SourceAccountNumber);
            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Error = "The originating account number is invalid.";
                return View(vm);
            }

            var loan = await _loanService.GetByLoanNumberAsync(vm.LoanNumber);
            if (loan == null || loan.Status != LoanStatus.Active)
            {
                vm.HasError = true;
                vm.Error = "The loan number entered is not valid.";
                return View(vm);
            }

            if (account.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Error = "The amount exceeds the available balance in the account.";
                return View(vm);
            }

            vm.LoanHolderName = loan.ClientFullName;
            return View("PayLoanConfirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayLoanConfirm(CashierPayLoanViewModel vm)
        {
            try
            {
                await _transactionService.CashierPayLoanAsync(new CashierPayLoanDto
                {
                    SourceAccountNumber = vm.SourceAccountNumber,
                    LoanNumber = vm.LoanNumber,
                    Amount = vm.Amount
                });

                TempData["Success"] = "Loan payment made. The customer was notified by email.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View("PayLoanConfirm", vm);
            }
        }

        #endregion

        #region third party payment

        public IActionResult ThirdPartyTransfer()
        {
            return View(new CashierThirdPartyTransferViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThirdPartyTransfer(CashierThirdPartyTransferViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var sourceAccount = await _savingsAccountService
                .GetByAccountNumberAsync(vm.SourceAccountNumber);

            if (sourceAccount == null || !sourceAccount.IsActive)
            {
                vm.HasError = true;
                vm.Error = "The originating account number is invalid.";
                return View(vm);
            }

            if (sourceAccount.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Error = "The amount exceeds the available balance in the originating account.";
                return View(vm);
            }

            var destAccount = await _savingsAccountService
                .GetByAccountNumberAsync(vm.DestinationAccountNumber);

            if (destAccount == null || !destAccount.IsActive)
            {
                vm.HasError = true;
                vm.Error = "The destination account number is invalid.";
                return View(vm);
            }

            vm.DestinationHolderName = destAccount.OwnerFullName;
            return View("ThirdPartyTransferConfirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThirdPartyTransferConfirm(CashierThirdPartyTransferViewModel vm)
        {
            try
            {
                await _transactionService.CashierTransferAsync(new CashierTransferDto
                {
                    SourceAccountNumber = vm.SourceAccountNumber,
                    DestinationAccountNumber = vm.DestinationAccountNumber,
                    Amount = vm.Amount
                });

                TempData["Success"] = "Transaction completed. Both parties were notified by mail.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                return View("ThirdPartyTransferConfirm", vm);
            }
        }

        #endregion
    }
}
