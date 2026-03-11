using ArtemisBank.Core.Application.DTOs.SavingsAccount;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ISavingsAccountService
    {
        Task<SavingsAccountDto> GetByIdAsync(int id);
        Task<IEnumerable<SavingsAccountDto>> GetAllAsync();
        Task AddAsync(SavingsAccountDto dto);
        Task UpdateAsync(SavingsAccountDto dto);
        Task DeleteAsync(int id);
    }
}
