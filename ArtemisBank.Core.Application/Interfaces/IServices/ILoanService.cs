using ArtemisBank.Core.Application.DTOs.Loan;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ILoanService
    {
        Task<LoanDto> GetByIdAsync(int id);
        Task<IEnumerable<LoanDto>> GetAllAsync();
        Task AddAsync(LoanDto dto);
        Task UpdateAsync(LoanDto dto);
        Task DeleteAsync(int id);
    }
}
