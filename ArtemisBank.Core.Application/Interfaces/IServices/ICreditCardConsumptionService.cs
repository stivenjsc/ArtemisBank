using ArtemisBank.Core.Application.DTOs.CreditCardConsumption;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ICreditCardConsumptionService
    {
        Task<CreditCardConsumptionDto> GetByIdAsync(int id);
        Task<IEnumerable<CreditCardConsumptionDto>> GetAllAsync();
        Task AddAsync(CreditCardConsumptionDto dto);
        Task UpdateAsync(CreditCardConsumptionDto dto);
        Task DeleteAsync(int id);
    }
}
