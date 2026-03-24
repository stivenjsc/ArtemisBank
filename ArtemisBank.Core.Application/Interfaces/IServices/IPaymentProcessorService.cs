using ArtemisBank.Core.Application.DTOs.Payment;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface IPaymentProcessorService
    {
        Task<PaymentResultDto> ProcessPaymentAsync(int commerceId, ProcessPaymentDto paymentDto);
        Task<IEnumerable<PaymentTransactionDto>> GetCommerceTransactionsAsync(int commerceId);
    }
}
