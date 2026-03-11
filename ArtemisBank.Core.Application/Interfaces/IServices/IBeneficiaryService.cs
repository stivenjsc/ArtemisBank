using ArtemisBank.Core.Application.DTOs.Beneficiary;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface IBeneficiaryService
    {
        Task<BeneficiaryDto> GetByIdAsync(int id);
        Task<IEnumerable<BeneficiaryDto>> GetAllAsync();
        Task AddAsync(BeneficiaryDto dto);
        Task UpdateAsync(BeneficiaryDto dto);
        Task DeleteAsync(int id);
    }
}
