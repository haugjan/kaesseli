using Kaesseli.Domain.Accounts;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Accounts;

internal class AccountRepository(KaesseliContext context) : IAccountRepository
{
    public async Task<Account> AddAccount(Account account, CancellationToken cancellationToken)
    {
        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken) =>
        await context.Accounts.OrderBy(account=>account.Name).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Account>> GetAccounts(AccountType accountType, CancellationToken cancellationToken) =>
        await context.Accounts.Where(account => account.Type == accountType).ToListAsync(cancellationToken);

    public async Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken) =>
        await context.Accounts.SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken)
     ?? throw new EntityNotFoundException(entityType: typeof(Account), accountId);

    public async Task<AccountingPeriod> GetAccountingPeriod(Guid accountingPeriodId, CancellationToken cancellationToken) =>
        await context.AccountingPeriods.FirstOrDefaultAsync(ap => ap.Id == accountingPeriodId, cancellationToken)
     ?? throw new EntityNotFoundException(entityType: typeof(AccountingPeriod), accountingPeriodId);

    public async Task<AccountingPeriod> AddAccountingPeriod(AccountingPeriod accountingPeriod, CancellationToken cancellationToken)
    {
        context.AccountingPeriods.Add(accountingPeriod);
        await context.SaveChangesAsync(cancellationToken);
        return accountingPeriod;
    }

    public async Task<IEnumerable<AccountingPeriod>> GetAccountingPeriods(CancellationToken cancellationToken) =>
        await context.AccountingPeriods.ToListAsync(cancellationToken);
}