using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Domain.Interfaces;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _repo;
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly IMapper _mapper;

        public CreditCardService(ICreditCardRepository repo, ISavingsAccountRepository accountRepo, IMapper mapper)
        {
            _repo = repo;
            _accountRepo = accountRepo;
            _mapper = mapper;
        }

        public async Task<CreditCardDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<CreditCardDto>(entity);
        }

        public async Task<CreditCardDto?> GetByCardNumberAsync(string cardNumber)
        {
            var entity = await _repo.GetByCardNumberAsync(cardNumber);
            return entity is null ? null : _mapper.Map<CreditCardDto>(entity);
        }

        public async Task<IEnumerable<CreditCardDto>> GetActiveByClientIdAsync(string clientId)
        {
            var entities = await _repo.GetActiveCardsByClientIdAsync(clientId);
            return _mapper.Map<IEnumerable<CreditCardDto>>(entities);
        }

        public async Task<PaginatedResult<CreditCardDto>> GetAllPagedAsync(int page, int pageSize = 20, CardStatus? status = null, string? cedula = null)
        {
            var entities = await _repo.GetAllPagedAsync(page, pageSize, status, cedula);
            var items = _mapper.Map<IEnumerable<CreditCardDto>>(entities);
            var totalCount = await _repo.GetTotalActiveCardsCountAsync();

            return new PaginatedResult<CreditCardDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CreditCardDto> AssignAsync(AssignCreditCardDto dto)
        {
            string cardNumber;
            do
            {
                cardNumber = GenerateCardNumber();
            }
            while (await _repo.CardNumberExistsAsync(cardNumber));

            var cvc = Random.Shared.Next(100, 999).ToString();
            var cvcHash = HashCvc(cvc);

            var card = new CreditCard
            {
                CardNumber = cardNumber,
                CreditLimit = dto.CreditLimit,
                ExpirationDate = DateTime.UtcNow.AddYears(5).ToString("MM/yy"),
                AmountOwed = 0,
                CVCHash = cvcHash,
                Status = CardStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ClientId = dto.ClientId,
                AssignedByAdminId = string.Empty
            };

            await _repo.AddAsync(card);
            return _mapper.Map<CreditCardDto>(card);
        }

        public async Task<bool> ChangeStatusAsync(int cardId, CardStatus status)
        {
            var entity = await _repo.GetByIdAsync(cardId);
            if (entity == null) return false;

            entity.Status = status;
            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> PayCreditCardAsync(string sourceAccountNumber, string cardNumber, decimal amount)
        {
            if (amount <= 0) return false;

            var account = await _accountRepo.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null || account.Status != AccountStatus.Active) return false;
            if (account.Balance < amount) return false;

            var card = await _repo.GetByCardNumberAsync(cardNumber);
            if (card == null || card.Status != CardStatus.Active) return false;

            var paymentAmount = Math.Min(amount, card.AmountOwed);
            account.Balance -= paymentAmount;
            card.AmountOwed -= paymentAmount;

            await _accountRepo.UpdateAsync(account);
            await _repo.UpdateAsync(card);
            return true;
        }

        public async Task<bool> CashAdvanceAsync(CashAdvanceDto dto)
        {
            if (dto.Amount <= 0) return false;

            var card = await _repo.GetByIdAsync(dto.CreditCardId);
            if (card == null || card.Status != CardStatus.Active) return false;

            var account = await _accountRepo.GetByIdAsync(dto.SavingsAccountId);
            if (account == null || account.Status != AccountStatus.Active) return false;

            var availableCredit = card.CreditLimit - card.AmountOwed;
            if (dto.Amount > availableCredit) return false;

            // 6.25% interest on cash advances
            var totalWithInterest = dto.Amount * 1.0625m;

            card.AmountOwed += totalWithInterest;
            account.Balance += dto.Amount;

            await _repo.UpdateAsync(card);
            await _accountRepo.UpdateAsync(account);
            return true;
        }

        public async Task<decimal> GetTotalDebtByClientIdAsync(string clientId)
        {
            return await _repo.GetTotalCardDebtByClientIdAsync(clientId);
        }

        public async Task<int> GetTotalActiveCardsCountAsync()
        {
            return await _repo.GetTotalActiveCardsCountAsync();
        }

        private static string GenerateCardNumber()
        {
            return $"{Random.Shared.Next(1000, 9999)}-{Random.Shared.Next(1000, 9999)}-{Random.Shared.Next(1000, 9999)}-{Random.Shared.Next(1000, 9999)}";
        }

        private static string HashCvc(string cvc)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(cvc));
            return Convert.ToHexStringLower(bytes);
        }
    }
}
