using ArtemisBank.Core.Application.DTOs.Transaction;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Transaction;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Client))]
    public class TransactionController(
        ITransactionService transactionService,
        ISavingsAccountService savingsAccountService,
        ICreditCardService creditCardService,
        ILoanService loanService,
        IBeneficiaryService beneficiaryService) : Controller
    {
        private readonly ITransactionService _transactionService = transactionService;
        private readonly ISavingsAccountService _savingsAccountService = savingsAccountService;
        private readonly ICreditCardService _creditCardService = creditCardService;
        private readonly ILoanService _loanService = loanService;
        private readonly IBeneficiaryService _beneficiaryService = beneficiaryService;

        private string GetClientId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        #region Transfer

        public async Task<IActionResult> Transfer()
        {
            var clientId = GetClientId();
            var vm = new TransferViewModel
            {
                UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferViewModel vm)
        {
            var clientId = GetClientId();

            if (!ModelState.IsValid)
            {
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                return View(vm);
            }

            try
            {
                await _transactionService.TransferAsync(new TransferDto
                {
                    SourceAccountNumber = vm.SourceAccountNumber,
                    DestinationAccountNumber = vm.DestinationAccountNumber,
                    Amount = vm.Amount
                });

                return RedirectToAction("Index", "Client");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                return View(vm);
            }
        }

        #endregion

        #region Express Payment

        public async Task<IActionResult> ExpressPayment()
        {
            var clientId = GetClientId();
            var vm = new ExpressPaymentViewModel
            {
                UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressPayment(ExpressPaymentViewModel vm)
        {
            var clientId = GetClientId();
            if (vm.SourceAccountNumber == vm.DestinationAccountNumber)
            {
                vm.HasError = true;
                vm.Error = "The source and destination accounts cannot be the same.";
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                return View(vm);
            }

            if (!ModelState.IsValid)
            {
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                return View(vm);
            }

            try
            {
                await _transactionService.PayExpressAsync(new PaymentDto
                {
                    SourceAccountNumber = vm.SourceAccountNumber,
                    DestinationAccountNumber = vm.DestinationAccountNumber,
                    Amount = vm.Amount
                });

                return RedirectToAction("Index", "Client");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                return View(vm);
            }
        }

        #endregion

        #region Credit Card Payment

        public async Task<IActionResult> CreditCardPayment()
        {
            var clientId = GetClientId();
            var vm = new CreditCardPaymentViewModel
            {
                UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId),
                UserCreditCards = await _creditCardService.GetActiveByClientIdAsync(clientId)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel vm)
        {
            var clientId = GetClientId();

            if (!ModelState.IsValid)
            {
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                vm.UserCreditCards = await _creditCardService.GetActiveByClientIdAsync(clientId);
                return View(vm);
            }

            try
            {
                await _transactionService.PayCreditCardAsync(new PaymentDto
                {
                    SourceAccountNumber = vm.SourceAccountNumber,
                    DestinationAccountNumber = vm.CreditCardNumber,
                    Amount = vm.Amount
                });

                return RedirectToAction("Index", "Client");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                vm.UserCreditCards = await _creditCardService.GetActiveByClientIdAsync(clientId);
                return View(vm);
            }
        }

        #endregion

        #region Loan Payment

        public async Task<IActionResult> LoanPayment()
        {
            var clientId = GetClientId();
            var vm = new LoanPaymentViewModel
            {
                UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId),
                UserLoans = await _loanService.GetActiveByClientIdAsync(clientId)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel vm)
        {
            var clientId = GetClientId();

            if (!ModelState.IsValid)
            {
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                vm.UserLoans = await _loanService.GetActiveByClientIdAsync(clientId);
                return View(vm);
            }

            try
            {
                // Cambiar para usar LoanService y aplicar el pago correctamente a la cuota
                await _loanService.PayLoanInstallmentAsync(vm.SourceAccountNumber, vm.LoanNumber, vm.Amount);

                return RedirectToAction("Index", "Client");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                vm.UserLoans = await _loanService.GetActiveByClientIdAsync(clientId);
                return View(vm);
            }
        }

        #endregion

        #region Beneficiary Payment

        public async Task<IActionResult> BeneficiaryPayment()
        {
            var clientId = GetClientId();
            var vm = new BeneficiaryPaymentViewModel
            {
                UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId),
                Beneficiaries = await _beneficiaryService.GetByOwnerIdAsync(clientId)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BeneficiaryPayment(BeneficiaryPaymentViewModel vm)
        {
            var clientId = GetClientId();

            if (!ModelState.IsValid)
            {
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                vm.Beneficiaries = await _beneficiaryService.GetByOwnerIdAsync(clientId);
                return View(vm);
            }

            try
            {
                var beneficiary = await _beneficiaryService.GetByIdAsync(vm.BeneficiaryId);

                if (beneficiary == null || beneficiary.OwnerId != clientId)
                {
                    return Forbid();
                }

                await _transactionService.TransferAsync(new TransferDto
                {
                    SourceAccountNumber = vm.SourceAccountNumber,
                    DestinationAccountNumber = beneficiary.AccountNumber,
                    Amount = vm.Amount
                });

                return RedirectToAction("Index", "Client");
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.Error = ex.Message;
                vm.UserAccounts = await _savingsAccountService.GetByClientIdAsync(clientId);
                vm.Beneficiaries = await _beneficiaryService.GetByOwnerIdAsync(clientId);
                return View(vm);
            }
        }

        #endregion
    }
}
