using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.Core.Domain.Interfaces;
using ArtemisBank.Infrastructure.Identity.Context;
using ArtemisBank.Infrastructure.Persistence.Context;
using ArtemisBank.Infrastructure.Persistence.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class LoanRepository(ArtemisBankDbContext context, IdentityContext identityContext) : GenericRepository<Loan>(context), ILoanRepository
    {
        private readonly IdentityContext _identityContext = identityContext;
        public async Task<bool> ClientHasActiveLoanAsync(string clientId)
        {
            return await _dbSet.AnyAsync(l => l.UserId == clientId && l.Status == LoanStatus.Active);
        }

        public async Task<IEnumerable<Loan>> GetActiveByClientIdAsync(string clientId)
        {
            return await _dbSet.Where(l => l.UserId == clientId && l.Status == LoanStatus.Active)
                .OrderByDescending(l => l.CreatedAt).ToListAsync();
        }

        public async Task<Loan?> GetActiveLoanByClientIdAsync(string clientId)
        {
            return await _dbSet.Include(l => l.Installments).FirstOrDefaultAsync(l => l.UserId == clientId && l.Status == LoanStatus.Active);
        }

        public async Task<IEnumerable<Loan>> GetAllByClientCedulaAsync(string cedula)
        {
            var clientId = await _identityContext.Users.Where(u => u.Cedula == cedula).Select(u => u.Id).FirstOrDefaultAsync();

            if (clientId == null)
            {
                return [];
            }
            return await _dbSet.AsNoTracking().Where(l => l.UserId == clientId)
                .OrderByDescending(l => l.Status == LoanStatus.Active).ThenByDescending(l=> l.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetAllPagedAsync(int page, int pageSize, LoanStatus? status = null, string? cedula = null)
        {
            var query = _dbSet.AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status);
            }
            if (!string.IsNullOrEmpty(cedula))
            {
                var clientId = await _identityContext.Users.Where(u => u.Cedula == cedula).Select(u => u.Id).FirstOrDefaultAsync();
                if (clientId != null)
                {
                    query = query.Where(l => l.UserId == clientId);
                }
                else
                {
                    return [];
                }
            }

            return await query.OrderByDescending(l => l.Status == LoanStatus.Active).ThenByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<decimal> GetAverageDebtAsync()
        {
            // average debt = total debt of active loans / number of active loans
            var clientWithDebt = await _dbSet.Where(l => l.Status == LoanStatus.Active).GroupBy(l => l.UserId)
                .Select(a => a.Sum(l => l.Installments.Where(i => i.Status != InstallmentStatus.Paid)
                .Sum(i => i.InstallmentAmount - i.AmountPaid))).ToListAsync();

            if (clientWithDebt.Count == 0) return 0;

            return clientWithDebt.Average();
        }

        public async Task<Loan?> GetByLoanNumberAsync(string loanNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(l => l.LoanNumber == loanNumber);
        }

        public async Task<int> GetTotalActiveLoansCountAsync()
        {
            return await _dbSet.CountAsync(l => l.Status == LoanStatus.Active);
        }

        public async Task<decimal> GetTotalDebtByClientIdAsync(string clientId)
        {
            // total debt = pending amount  + debt in credit cards
            var loanDebt = await _dbSet.Where(l => l.UserId == clientId && l.Status == LoanStatus.Active)
                .SelectMany(l => l.Installments).Where(i => i.Status != InstallmentStatus.Paid)
                .SumAsync(i => i.InstallmentAmount - i.AmountPaid);

            var cardDebt = await _context.CreditCards.Where(cc => cc.ClientId == clientId && cc.Status == CardStatus.Active)
                .SumAsync(cc => cc.AmountOwed);

            return loanDebt + cardDebt;
        }
    }
}
