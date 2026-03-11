using ArtemisBank.Core.Application.DTOs.LoanInstallment;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ILoanInstallmentService
    {
        Task<LoanInstallmentDto> GetByIdAsync(int id);
        Task<IEnumerable<LoanInstallmentDto>> GetAllAsync();
        Task AddAsync(LoanInstallmentDto dto);
        Task UpdateAsync(LoanInstallmentDto dto);
        Task DeleteAsync(int id);
    }
}
