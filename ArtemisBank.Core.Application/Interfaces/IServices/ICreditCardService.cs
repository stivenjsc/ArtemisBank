using ArtemisBank.Core.Application.DTOs.CreditCard;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ICreditCardService
    {
        Task<CreditCardDto> GetByIdAsync(int id);
        Task<IEnumerable<CreditCardDto>> GetAllAsync();
        Task AddAsync(CreditCardDto dto);
        Task UpdateAsync(CreditCardDto dto);
        Task DeleteAsync(int id);
    }
}
