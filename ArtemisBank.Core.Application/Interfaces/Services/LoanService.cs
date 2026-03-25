using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Domain.Interfaces;
using AutoMapper;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public class LoanService(ILoanRepository repo, ILoanInstallmentRepository installmentRepo, ITransactionRepository transactionrepo, 
        ISavingsAccountRepository accountRepo, IUserReadOnlyService user, IMapper mapper) : ILoanService
    {
        private readonly ILoanRepository _repo = repo;
        private readonly ILoanInstallmentRepository _installmentRepo = installmentRepo;
        private readonly ITransactionRepository _transactionRepo = transactionrepo;
        private readonly ISavingsAccountRepository _accountRepo = accountRepo;
        private readonly IUserReadOnlyService _userService = user;
        private readonly IMapper _mapper = mapper;

        public async Task<LoanDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<LoanDto>(entity);
        }

        public async Task<LoanDto?> GetByLoanNumberAsync(string loanNumber)
        {
            var entity = await _repo.GetByLoanNumberAsync(loanNumber);
            return entity is null ? null : _mapper.Map<LoanDto>(entity);
        }

        public async Task<IEnumerable<LoanDto>> GetActiveByClientIdAsync(string clientId)
        {
            var entities = await _repo.GetActiveByClientIdAsync(clientId);
            return _mapper.Map<IEnumerable<LoanDto>>(entities);
        }

        public async Task<PaginatedResult<LoanDto>> GetAllPagedAsync(int page, int pageSize = 20, LoanStatus? status = null, string? cedula = null)
        {
            var entities = await _repo.GetAllPagedAsync(page, pageSize, status, cedula);
            var items = _mapper.Map<IEnumerable<LoanDto>>(entities);

            foreach (var item in items)
            {
                var user = await _userService.GetByIdAsync(item.ClientId);
                if (user != null)
                    item.ClientFullName = $"{user.FirstName} {user.LastName}";
            }
            var totalCount = await _repo.GetTotalActiveLoansCountAsync();

            return new PaginatedResult<LoanDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<IEnumerable<UserDto>> GetActiveClientsWithoutLoanAsync(string? cedula = null)
        {
            var allActiveClients = await _userService.GetActiveClientsAsync(cedula);

            // 2. Filtrar usando la lógica de préstamos que ya conoce este servicio
            var filteredClients = new List<UserDto>();
            foreach (var client in allActiveClients)
            {
                var hasActiveLoan = await _repo.ClientHasActiveLoanAsync(client.Id);
                if (!hasActiveLoan)
                {
                    filteredClients.Add(client);
                }
            }
            return filteredClients;
        }
        public async Task<LoanDto> AssignAsync(AssignLoanDto dto)
        {
            if (await _repo.ClientHasActiveLoanAsync(dto.ClientId))
                throw new InvalidOperationException("Client already has an active loan.");

            string loanNumber;
            do
            {
                loanNumber = Random.Shared.Next(100000000, 999999999).ToString();
            }
            while (await _repo.GetByLoanNumberAsync(loanNumber) != null);

            var loan = new Loan
            {
                LoanNumber = loanNumber,
                Amount = dto.Amount,
                AnualInterestRate = dto.AnnualInterestRate,
                TermInMonths = dto.TermInMonths,
                Status = LoanStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ClientId = dto.ClientId,
                AssignedByAdminId = dto.AdminId
            };

            await _repo.AddAsync(loan);

            // French amortization: fixed monthly payment
            var totalDebt = CalculateTotalLoanDebt(dto.Amount, dto.AnnualInterestRate, dto.TermInMonths);
            var fixedPayment = totalDebt / dto.TermInMonths;

            for (int i = 1; i <= dto.TermInMonths; i++)
            {
                var installment = new LoanInstallment
                {
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    InstallmentAmount = Math.Round(fixedPayment, 2),
                    AmountPaid = 0,
                    Status = InstallmentStatus.Pending,
                    IsOverdue = false,
                    InstallmentNumber = i,
                    LoanId = loan.Id
                };

                await _installmentRepo.AddAsync(installment);
            }

            // Deposit loan amount into client's primary account
            var primaryAccount = await _accountRepo.GetPrimaryAccountByClientIdAsync(dto.ClientId);
            if (primaryAccount != null)
            {
                primaryAccount.Balance += dto.Amount;
                await _accountRepo.UpdateAsync(primaryAccount);

                await _transactionRepo.AddAsync(new Transaction
                {
                    Amount = dto.Amount,
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.Credit,
                    Origin = loan.LoanNumber,
                    Beneficiary = primaryAccount.AccountNumber,
                    SourceAccountNumber = loan.LoanNumber,
                    DestinationAccountNumber = primaryAccount.AccountNumber,
                    Description = $"Loan disbursement {loan.LoanNumber}",
                    Status = TransactionStatus.Approved,
                    SavingAccountId = primaryAccount.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return _mapper.Map<LoanDto>(loan);
        }

        public async Task<bool> PayLoanInstallmentAsync(string sourceAccountNumber, string loanNumber, decimal amount)
        {
            if (amount <= 0) return false;

            var account = await _accountRepo.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null || account.Status != AccountStatus.Active) return false;
            if (account.Balance < amount) return false;

            var loan = await _repo.GetByLoanNumberAsync(loanNumber);
            if (loan == null || loan.Status != LoanStatus.Active) return false;

            var installment = await _installmentRepo.GetFirstPendingInstallmentAsync(loan.Id);
            if (installment == null) return false;

            var remaining = installment.InstallmentAmount - installment.AmountPaid;
            var paymentAmount = Math.Min(amount, remaining);

            account.Balance -= paymentAmount;
            installment.AmountPaid += paymentAmount;

            if (installment.AmountPaid >= installment.InstallmentAmount)
            {
                installment.Status = InstallmentStatus.Paid;
            }

            await _accountRepo.UpdateAsync(account);
            await _installmentRepo.UpdateAsync(installment);

            // Check if all installments are paid
            var pendingAmount = await _installmentRepo.GetPendingAmountByLoanIdAsync(loan.Id);
            if (pendingAmount <= 0)
            {
                loan.Status = LoanStatus.Completed;
                await _repo.UpdateAsync(loan);
            }

            return true;
        }

        public async Task<bool> ClientHasActiveLoanAsync(string clientId)
        {
            return await _repo.ClientHasActiveLoanAsync(clientId);
        }

        public async Task<decimal> GetTotalDebtByClientIdAsync(string clientId)
        {
            return await _repo.GetTotalDebtByClientIdAsync(clientId);
        }

        public async Task<decimal> GetAverageDebtAsync()
        {
            return await _repo.GetAverageDebtAsync();
        }

        public async Task<int> GetTotalActiveLoansCountAsync()
        {
            return await _repo.GetTotalActiveLoansCountAsync();
        }

        #region private helper methods
        private static decimal CalculateTotalLoanDebt(decimal amount, decimal annualRate, int months)
        {
            if (months <= 0) return amount;
            if (annualRate == 0) return amount;

            double monthlyRate = (double)annualRate / 100.0 / 12.0;
            double p = (double)amount;
            double n = months;

            double factor = Math.Pow(1 + monthlyRate, n);
            double monthlyPayment = p * (monthlyRate * factor) / (factor - 1);

            return (decimal)(monthlyPayment * n);
        }

        public async Task<(bool IsHighRisk, decimal AverageDebt, decimal CurrentDebt)> EvaluateRiskAsync(string clientId, decimal amount, decimal rate, int months)
        {
            var averageDebt = await GetAverageDebtAsync();
            var currentDebt = await GetTotalDebtByClientIdAsync(clientId);

            var totalNewLoanDebt = CalculateTotalLoanDebt(amount, rate, months);
            var newTotalDebt = currentDebt + totalNewLoanDebt;

            bool isHighRisk = currentDebt > averageDebt || newTotalDebt > averageDebt;

            return (isHighRisk, averageDebt, currentDebt);
        }

        public async Task UpdateInterestRateAsync(int loanId, decimal newAnnualInterestRate)
        {
            var loan = await _repo.GetByIdAsync(loanId);
            if (loan == null) throw new Exception("Loan not found");

            loan.AnualInterestRate = newAnnualInterestRate;
            await _repo.UpdateAsync(loan);

            var pendingInstallments = (await _installmentRepo.GetByLoanIdAsync(loanId))
                .Where(i => i.Status != InstallmentStatus.Paid).OrderBy(i => i.InstallmentNumber).ToList();
            if (!pendingInstallments.Any()) return;

            decimal remainingBalance = pendingInstallments.Sum(i => i.InstallmentAmount - i.AmountPaid);
            int remainingMonths = pendingInstallments.Count;

            double monthlyRate = (double)newAnnualInterestRate / 100 / 12;
            decimal newFixedPayment;

            if (monthlyRate == 0)
            {
                newFixedPayment = remainingBalance / remainingMonths;
            }
            else
            {
                double factor = Math.Pow(1 + monthlyRate, remainingMonths);
                double monthlyPayment = (double)remainingBalance * (monthlyRate * factor) / (factor - 1);
                newFixedPayment = (decimal)monthlyPayment;
            }

            foreach (var installment in pendingInstallments)
            {
                installment.InstallmentAmount = Math.Round(newFixedPayment, 2);
                await _installmentRepo.UpdateAsync(installment);
            }

            loan.AnualInterestRate = newAnnualInterestRate;
            await _repo.UpdateAsync(loan);
        }

        #endregion
    }
}
