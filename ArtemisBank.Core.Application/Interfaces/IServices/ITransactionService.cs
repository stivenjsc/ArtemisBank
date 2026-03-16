using ArtemisBank.Core.Application.DTOs.Transaction;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface ITransactionService
    {
        Task<TransactionDto> GetByIdAsync(int id);
        Task<IEnumerable<TransactionDto>> GetByAccountIdAsync(int savingsAccountId);

        Task<TransactionDto> TransferAsync(TransferDto dto);
        Task<TransactionDto> PayExpressAsync(PaymentDto dto);
        Task<TransactionDto> PayCreditCardAsync(PaymentDto dto);
        Task<TransactionDto> PayLoanAsync(PaymentDto dto);

        Task<int> GetTodayTransactionsCountAsync();
        Task<int> GetTotalTransactionsCountAsync();
        Task<int> GetTodayPaymentsCountAsync();
        Task<int> GetTotalPaymentsCountAsync();
    }
}
