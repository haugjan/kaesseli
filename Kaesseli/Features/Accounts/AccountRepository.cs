using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Features.Accounts;

public interface IAccountRepository
{
    Task<Account> AddAccount(Account account, CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(
        AccountType accountType,
        CancellationToken cancellationToken
    );
    Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken);
    Task<AccountingPeriod> GetAccountingPeriod(
        Guid accountingPeriodId,
        CancellationToken cancellationToken
    );
    Task<AccountingPeriod> AddAccountingPeriod(
        AccountingPeriod accountingPeriod,
        CancellationToken cancellationToken
    );
    Task<IEnumerable<AccountingPeriod>> GetAccountingPeriods(CancellationToken cancellationToken);
    Task UpdateAccountingPeriod(
        AccountingPeriod accountingPeriod,
        CancellationToken cancellationToken
    );
    Task DeleteAccountingPeriod(Guid accountingPeriodId, CancellationToken cancellationToken);
    Task UpdateAccount(Account account, CancellationToken cancellationToken);
    Task DeleteAccount(Guid accountId, CancellationToken cancellationToken);
    Task<bool> AccountNumberExists(
        string number,
        Guid? excludeAccountId,
        CancellationToken cancellationToken
    );
    Task<bool> AccountShortNameExists(
        string shortName,
        Guid? excludeAccountId,
        CancellationToken cancellationToken
    );
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
        await context.Accounts.OrderBy(account => account.Name).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Account>> GetAccounts(
        AccountType accountType,
        CancellationToken cancellationToken
    ) =>
        await context
            .Accounts.Where(account => account.Type == accountType)
            .ToListAsync(cancellationToken);

    public async Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken) =>
        await context.Accounts.SingleOrDefaultAsync(
            account => account.Id == accountId,
            cancellationToken
        ) ?? throw new EntityNotFoundException(entityType: typeof(Account), accountId);

    public async Task<AccountingPeriod> GetAccountingPeriod(
        Guid accountingPeriodId,
        CancellationToken cancellationToken
    ) =>
        await context.AccountingPeriods.FirstOrDefaultAsync(
            ap => ap.Id == accountingPeriodId,
            cancellationToken
        )
        ?? throw new EntityNotFoundException(
            entityType: typeof(AccountingPeriod),
            accountingPeriodId
        );

    public async Task<AccountingPeriod> AddAccountingPeriod(
        AccountingPeriod accountingPeriod,
        CancellationToken cancellationToken
    )
    {
        context.AccountingPeriods.Add(accountingPeriod);
        await context.SaveChangesAsync(cancellationToken);
        return accountingPeriod;
    }

    public async Task<IEnumerable<AccountingPeriod>> GetAccountingPeriods(
        CancellationToken cancellationToken
    ) => await context.AccountingPeriods.ToListAsync(cancellationToken);

    public async Task UpdateAccountingPeriod(
        AccountingPeriod accountingPeriod,
        CancellationToken cancellationToken
    )
    {
        context.AccountingPeriods.Update(accountingPeriod);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAccountingPeriod(
        Guid accountingPeriodId,
        CancellationToken cancellationToken
    )
    {
        var period =
            await context.AccountingPeriods.FirstOrDefaultAsync(
                ap => ap.Id == accountingPeriodId,
                cancellationToken
            ) ?? throw new EntityNotFoundException(typeof(AccountingPeriod), accountingPeriodId);

        var journalEntries = await context
            .JournalEntries.Where(e =>
                EF.Property<Guid>(e, "AccountingPeriodId") == accountingPeriodId
            )
            .ToListAsync(cancellationToken);
        context.JournalEntries.RemoveRange(journalEntries);

        var budgetEntries = await context
            .BudgetEntries.Where(e =>
                EF.Property<Guid>(e, "AccountingPeriodId") == accountingPeriodId
            )
            .ToListAsync(cancellationToken);
        context.BudgetEntries.RemoveRange(budgetEntries);

        context.AccountingPeriods.Remove(period);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAccount(Account account, CancellationToken cancellationToken)
    {
        context.Accounts.Update(account);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAccount(Guid accountId, CancellationToken cancellationToken)
    {
        var account =
            await context.Accounts.SingleOrDefaultAsync(a => a.Id == accountId, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Account), accountId);

        context.Accounts.Remove(account);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> AccountNumberExists(
        string number,
        Guid? excludeAccountId,
        CancellationToken cancellationToken
    ) =>
        context.Accounts.AnyAsync(
            a => a.Number == number && (excludeAccountId == null || a.Id != excludeAccountId),
            cancellationToken
        );

    public Task<bool> AccountShortNameExists(
        string shortName,
        Guid? excludeAccountId,
        CancellationToken cancellationToken
    ) =>
        context.Accounts.AnyAsync(
            a => a.ShortName == shortName && (excludeAccountId == null || a.Id != excludeAccountId),
            cancellationToken
        );
}
