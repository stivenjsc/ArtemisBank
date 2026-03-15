using AutoMapper;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Domain.Interfaces;

namespace ArtemisBank.Core.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _repo;
        private readonly ILoanInstallmentRepository _installmentRepo;
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly IMapper _mapper;

        public LoanService(ILoanRepository repo, ILoanInstallmentRepository installmentRepo, ISavingsAccountRepository accountRepo, IMapper mapper)
        {
            _repo = repo;
            _installmentRepo = installmentRepo;
            _accountRepo = accountRepo;
            _mapper = mapper;
        }

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
            var totalCount = await _repo.GetTotalActiveLoansCountAsync();

            return new PaginatedResult<LoanDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<LoanDto> AssignAsync(AssignLoanDto dto)
        {
            if (await _repo.ClientHasActiveLoanAsync(dto.ClientId))
                throw new InvalidOperationException("Client already has an active loan.");

            string loanNumber;
            do
            {
                loanNumber = $"LN{Random.Shared.Next(100000000, 999999999)}";
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
                UserId = dto.ClientId,
                AssignedByAdminId = string.Empty
            };

            await _repo.AddAsync(loan);

            // French amortization: fixed monthly payment
            var monthlyRate = dto.AnnualInterestRate / 100m / 12m;
            var fixedPayment = dto.Amount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), dto.TermInMonths))
                              / ((decimal)Math.Pow((double)(1 + monthlyRate), dto.TermInMonths) - 1);

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
    }
}
