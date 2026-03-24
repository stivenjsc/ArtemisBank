using ArtemisBank.Core.Application.DTOs.SavingsAccount;
using ArtemisBank.Core.Application.DTOs.Transaction;

namespace ArtemisBank.Core.Application.ViewModels.Account
{
    public class AccountDetailViewModel
    {
        public SavingsAccountDto Account { get; set; } = new SavingsAccountDto();
        public IEnumerable<TransactionDto> Transactions { get; set; } = Enumerable.Empty<TransactionDto>();
    }
}
