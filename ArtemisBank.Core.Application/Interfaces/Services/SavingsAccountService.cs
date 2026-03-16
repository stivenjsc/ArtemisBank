using AutoMapper;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.SavingsAccount;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Domain.Interfaces;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public class SavingsAccountService : ISavingsAccountService
    {
        private readonly ISavingsAccountRepository _repo;
        private readonly IMapper _mapper;

        public SavingsAccountService(ISavingsAccountRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<SavingsAccountDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<SavingsAccountDto>(entity);
        }

        public async Task<SavingsAccountDto?> GetByAccountNumberAsync(string accountNumber)
        {
            var entity = await _repo.GetByAccountNumberAsync(accountNumber);
            return entity is null ? null : _mapper.Map<SavingsAccountDto>(entity);
        }

        public async Task<IEnumerable<SavingsAccountDto>> GetByClientIdAsync(string clientId)
        {
            var entities = await _repo.GetAllAccountByClienteIdAsync(clientId);
            return _mapper.Map<IEnumerable<SavingsAccountDto>>(entities);
        }

        public async Task<SavingsAccountDto?> GetPrimaryAccountByClientIdAsync(string clientId)
        {
            var entity = await _repo.GetPrimaryAccountByClientIdAsync(clientId);
            return entity is null ? null : _mapper.Map<SavingsAccountDto>(entity);
        }

        public async Task<PaginatedResult<SavingsAccountDto>> GetAllPagedAsync(int page, int pageSize = 20, AccountStatus? status = null, AccountType? type = null)
        {
            var entities = await _repo.GetAllPagedAsync(page, pageSize, status, type);
            var items = _mapper.Map<IEnumerable<SavingsAccountDto>>(entities);
            var totalCount = await _repo.GetTotalActiveAccountsCountAsync();

            return new PaginatedResult<SavingsAccountDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<SavingsAccountDto> CreateAccountAsync(string clientId, decimal initialAmount, AccountType type = AccountType.Primary)
        {
            string accountNumber;
            do
            {
                accountNumber = GenerateAccountNumber();
            }
            while (await _repo.AccountOrLoanNumberExistsAsync(accountNumber));

            var account = new SavingsAccount
            {
                AccountNumber = accountNumber,
                Balance = initialAmount,
                Type = type,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UserId = clientId
            };

            await _repo.AddAsync(account);
            return _mapper.Map<SavingsAccountDto>(account);
        }

        public async Task UpdateAsync(SavingsAccountDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null) return;

            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
        }

        public async Task<bool> ChangeStatusAsync(int accountId, AccountStatus status)
        {
            var entity = await _repo.GetByIdAsync(accountId);
            if (entity == null) return false;

            entity.Status = status;
            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DepositAsync(string accountNumber, decimal amount)
        {
            if (amount <= 0) return false;

            var account = await _repo.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.Status != AccountStatus.Active) return false;

            account.Balance += amount;
            await _repo.UpdateAsync(account);
            return true;
        }

        public async Task<bool> WithdrawAsync(string accountNumber, decimal amount)
        {
            if (amount <= 0) return false;

            var account = await _repo.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.Status != AccountStatus.Active) return false;
            if (account.Balance < amount) return false;

            account.Balance -= amount;
            await _repo.UpdateAsync(account);
            return true;
        }

        public async Task<bool> TransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount)
        {
            if (amount <= 0) return false;

            var source = await _repo.GetByAccountNumberAsync(sourceAccountNumber);
            var destination = await _repo.GetByAccountNumberAsync(destinationAccountNumber);

            if (source == null || destination == null) return false;
            if (source.Status != AccountStatus.Active || destination.Status != AccountStatus.Active) return false;
            if (source.Balance < amount) return false;

            source.Balance -= amount;
            destination.Balance += amount;

            await _repo.UpdateAsync(source);
            await _repo.UpdateAsync(destination);
            return true;
        }

        public async Task<bool> AccountNumberExistsAsync(string accountNumber)
        {
            return await _repo.AccountOrLoanNumberExistsAsync(accountNumber);
        }

        public async Task<int> GetTotalActiveAccountsCountAsync()
        {
            return await _repo.GetTotalActiveAccountsCountAsync();
        }

        private static string GenerateAccountNumber()
        {
            return $"ATB{Random.Shared.Next(100000000, 999999999)}";
        }
    }
}
