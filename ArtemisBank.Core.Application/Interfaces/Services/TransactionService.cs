using AutoMapper;
using ArtemisBank.Core.Application.DTOs.Transaction;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Domain.Interfaces;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repo;
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository repo, ISavingsAccountRepository accountRepo, IMapper mapper)
        {
            _repo = repo;
            _accountRepo = accountRepo;
            _mapper = mapper;
        }

        public async Task<TransactionDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<TransactionDto>(entity);
        }

        public async Task<IEnumerable<TransactionDto>> GetByAccountIdAsync(int savingsAccountId)
        {
            var entities = await _repo.GetByAccountIdAsync(savingsAccountId);
            return _mapper.Map<IEnumerable<TransactionDto>>(entities);
        }

        public async Task<TransactionDto> TransferAsync(TransferDto dto)
        {
            var source = await _accountRepo.GetByAccountNumberAsync(dto.SourceAccountNumber)
                ?? throw new InvalidOperationException("Source account not found.");

            var destination = await _accountRepo.GetByAccountNumberAsync(dto.DestinationAccountNumber)
                ?? throw new InvalidOperationException("Destination account not found.");

            if (source.Status != AccountStatus.Active || destination.Status != AccountStatus.Active)
                throw new InvalidOperationException("Both accounts must be active.");

            if (source.Balance < dto.Amount)
                throw new InvalidOperationException("Insufficient funds.");

            source.Balance -= dto.Amount;
            destination.Balance += dto.Amount;

            await _accountRepo.UpdateAsync(source);
            await _accountRepo.UpdateAsync(destination);

            var transaction = new Transaction
            {
                Amount = dto.Amount,
                TransactionDate = DateTime.UtcNow,
                Type = TransactionType.Debit,
                Origin = dto.SourceAccountNumber,
                Beneficiary = dto.DestinationAccountNumber,
                Status = TransactionStatus.Approved,
                SavingAccountId = source.Id
            };

            await _repo.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<TransactionDto> PayExpressAsync(PaymentDto dto)
        {
            var source = await _accountRepo.GetByAccountNumberAsync(dto.SourceAccountNumber)
                ?? throw new InvalidOperationException("Source account not found.");

            var destination = await _accountRepo.GetByAccountNumberAsync(dto.DestinationAccountNumber)
                ?? throw new InvalidOperationException("Destination account not found.");

            if (source.Status != AccountStatus.Active)
                throw new InvalidOperationException("Source account is not active.");

            if (source.Balance < dto.Amount)
                throw new InvalidOperationException("Insufficient funds.");

            source.Balance -= dto.Amount;
            destination.Balance += dto.Amount;

            await _accountRepo.UpdateAsync(source);
            await _accountRepo.UpdateAsync(destination);

            var transaction = new Transaction
            {
                Amount = dto.Amount,
                TransactionDate = DateTime.UtcNow,
                Type = TransactionType.Debit,
                Origin = dto.SourceAccountNumber,
                Beneficiary = dto.DestinationAccountNumber,
                Status = TransactionStatus.Approved,
                SavingAccountId = source.Id
            };

            await _repo.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<TransactionDto> PayCreditCardAsync(PaymentDto dto)
        {
            var source = await _accountRepo.GetByAccountNumberAsync(dto.SourceAccountNumber)
                ?? throw new InvalidOperationException("Source account not found.");

            if (source.Status != AccountStatus.Active)
                throw new InvalidOperationException("Source account is not active.");

            if (source.Balance < dto.Amount)
                throw new InvalidOperationException("Insufficient funds.");

            source.Balance -= dto.Amount;
            await _accountRepo.UpdateAsync(source);

            var transaction = new Transaction
            {
                Amount = dto.Amount,
                TransactionDate = DateTime.UtcNow,
                Type = TransactionType.Credit,
                Origin = dto.SourceAccountNumber,
                Beneficiary = dto.DestinationAccountNumber,
                Status = TransactionStatus.Approved,
                SavingAccountId = source.Id
            };

            await _repo.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<TransactionDto> PayLoanAsync(PaymentDto dto)
        {
            var source = await _accountRepo.GetByAccountNumberAsync(dto.SourceAccountNumber)
                ?? throw new InvalidOperationException("Source account not found.");

            if (source.Status != AccountStatus.Active)
                throw new InvalidOperationException("Source account is not active.");

            if (source.Balance < dto.Amount)
                throw new InvalidOperationException("Insufficient funds.");

            source.Balance -= dto.Amount;
            await _accountRepo.UpdateAsync(source);

            var transaction = new Transaction
            {
                Amount = dto.Amount,
                TransactionDate = DateTime.UtcNow,
                Type = TransactionType.Credit,
                Origin = dto.SourceAccountNumber,
                Beneficiary = dto.DestinationAccountNumber,
                Status = TransactionStatus.Approved,
                SavingAccountId = source.Id
            };

            await _repo.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<int> GetTodayTransactionsCountAsync()
        {
            return await _repo.GetTodayTransactionsCountAsync();
        }

        public async Task<int> GetTotalTransactionsCountAsync()
        {
            return await _repo.GetTotalTransactionsCountAsync();
        }

        public async Task<int> GetTodayPaymentsCountAsync()
        {
            return await _repo.GetTodayPaymentsCountAsync();
        }

        public async Task<int> GetTotalPaymentsCountAsync()
        {
            return await _repo.GetTotalPaymentsCountAsync();
        }
    }
}
