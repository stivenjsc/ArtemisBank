using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Domain.Enums;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ICreditCardService
    {
        Task<CreditCardDto> GetByIdAsync(int id);
        Task<CreditCardDto?> GetByCardNumberAsync(string cardNumber);
        Task<IEnumerable<CreditCardDto>> GetActiveByClientIdAsync(string clientId);
        Task<PaginatedResult<CreditCardDto>> GetAllPagedAsync(int page, int pageSize = 20, CardStatus? status = null, string? cedula = null);

        Task<CreditCardDto> AssignAsync(AssignCreditCardDto dto);
        Task<bool> ChangeStatusAsync(int cardId, CardStatus status);

        Task<bool> PayCreditCardAsync(string sourceAccountNumber, string cardNumber, decimal amount);
        Task<bool> CashAdvanceAsync(CashAdvanceDto dto);

        Task<decimal> GetTotalDebtByClientIdAsync(string clientId);
        Task<int> GetTotalActiveCardsCountAsync();
    }
}
