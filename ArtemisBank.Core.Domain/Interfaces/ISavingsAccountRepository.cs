using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Domain.Interfaces.IGenerics;

namespace ArtemisBank.Core.Domain.Interfaces
{
    public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
    {
        Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber);
        Task<SavingsAccount?> GetPrimaryAccountByClientIdAsync(string clientId);
        Task<IEnumerable<SavingsAccount>> GetActiveAccountsByClientIdAsync(string customerId);
        Task<IEnumerable<SavingsAccount>> GetAllAccountByClienteIdAsync(string clientId);
        Task<bool> AccountOrLoanNumberExistsAsync(string number);
        Task<IEnumerable<SavingsAccount>> GetAllPagedAsync(int page, int pageSize, AccountStatus? status = null, AccountType? type = null);
        Task<IEnumerable<SavingsAccount>> GetByClientCedulaAsync(string cedula, AccountStatus? status = null, AccountType? type = null);
        Task<int> GetTotalActiveAccountsCountAsync();
    }
}
