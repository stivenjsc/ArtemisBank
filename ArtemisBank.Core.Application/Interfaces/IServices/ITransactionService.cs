using ArtemisBank.Core.Application.DTOs.Transaction;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ITransactionService
    {
        Task<TransactionDto> GetByIdAsync(int id);
        Task<IEnumerable<TransactionDto>> GetAllAsync();
        Task AddAsync(TransactionDto dto);
        Task UpdateAsync(TransactionDto dto);
        Task DeleteAsync(int id);
    }
}
