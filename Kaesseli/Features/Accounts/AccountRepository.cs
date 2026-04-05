using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Features.Accounts;

public interface IAccountRepository
{
    Task<Account> AddAccount(Account account, CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(AccountType accountType, CancellationToken cancellationToken);
    Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken);
    Task<AccountingPeriod> GetAccountingPeriod(Guid accountingPeriodId, CancellationToken cancellationToken);
    Task<AccountingPeriod> AddAccountingPeriod(AccountingPeriod accountingPeriod, CancellationToken cancellationToken);
    Task<IEnumerable<AccountingPeriod>> GetAccountingPeriods(CancellationToken cancellationToken);
}

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
