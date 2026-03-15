using AutoMapper;
using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.DTOs.Dashboard;
using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.DTOs.SavingsAccount;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Interfaces;

namespace ArtemisBank.Core.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly ICreditCardRepository _cardRepo;
        private readonly ILoanRepository _loanRepo;
        private readonly IMapper _mapper;

        public DashboardService(
            ITransactionRepository transactionRepo,
            ISavingsAccountRepository accountRepo,
            ICreditCardRepository cardRepo,
            ILoanRepository loanRepo,
            IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
            _cardRepo = cardRepo;
            _loanRepo = loanRepo;
            _mapper = mapper;
        }

        public async Task<DashboardAdminDto> GetAdminDashboardAsync()
        {
            return new DashboardAdminDto
            {
                TotalTransactions = await _transactionRepo.GetTotalTransactionsCountAsync(),
                TotalActiveTransactions = await _transactionRepo.GetTodayTransactionsCountAsync(),
                TotalInactiveTransactions = 0,
                TotalDailyPayments = await _transactionRepo.GetTodayPaymentsCountAsync(),
                TotalAssignedProducts = await _accountRepo.GetTotalActiveAccountsCountAsync()
                    + await _cardRepo.GetTotalActiveCardsCountAsync()
                    + await _loanRepo.GetTotalActiveLoansCountAsync(),
                TotalActiveClients = 0,
                TotalInactiveClients = 0
            };
        }

        public async Task<DashboardClientDto> GetClientDashboardAsync(string clientId)
        {
            var accounts = await _accountRepo.GetAllAccountByClienteIdAsync(clientId);
            var cards = await _cardRepo.GetActiveCardsByClientIdAsync(clientId);
            var loans = await _loanRepo.GetActiveByClientIdAsync(clientId);

            return new DashboardClientDto
            {
                TotalSavingsAccounts = accounts.Count(),
                TotalCreditCards = cards.Count(),
                TotalLoans = loans.Count(),
                SavingsAccounts = _mapper.Map<IEnumerable<SavingsAccountDto>>(accounts),
                CreditCards = _mapper.Map<IEnumerable<CreditCardDto>>(cards),
                Loans = _mapper.Map<IEnumerable<LoanDto>>(loans)
            };
        }
    }
}
