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
    Task<OrphanCleanupCounts> CleanupOrphanedAccountReferences(CancellationToken cancellationToken);
}

public record OrphanCleanupCounts(int JournalEntriesDeleted, int BudgetEntriesDeleted);

internal class AccountRepository(KaesseliContext context) : IAccountRepository
{
    public async Task<Account> AddAccount(Account account, CancellationToken cancellationToken)
    {
        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken) =>
        await context.Accounts.OrderBy(account => account.Number).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Account>> GetAccounts(
        AccountType accountType,
        CancellationToken cancellationToken
    ) =>
        await context
            .Accounts.Where(account => account.Type == accountType)
            .OrderBy(account => account.Number)
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

        // EF Cosmos 10.0.5 generates malformed SQL for AnyAsync with a predicate
        // (see AccountNumberExists). Use Where(...).ToListAsync(...) and check in memory.
        var referencingEntries = await context
            .JournalEntries.Where(e =>
                EF.Property<Guid>(e, "DebitAccountId") == accountId
                || EF.Property<Guid>(e, "CreditAccountId") == accountId
            )
            .ToListAsync(cancellationToken);
        if (referencingEntries.Count > 0)
            throw new AccountInUseException(accountId);

        var budgetEntries = await context
            .BudgetEntries.Where(e => EF.Property<Guid>(e, "AccountId") == accountId)
            .ToListAsync(cancellationToken);
        context.BudgetEntries.RemoveRange(budgetEntries);

        context.Accounts.Remove(account);
        await context.SaveChangesAsync(cancellationToken);
    }

    // EF Cosmos 10.0.5 generates malformed 'SELECT VALUE EXISTS(SELECT 1 FROM root c ...)'
    // for both AnyAsync and Take(1)+ToListAsync with predicates including a Where clause.
    // Cosmos rejects this with "Identifier 'root' could not be resolved".
    // Workaround: fetch the full account set (small in this domain) and filter in memory.
    public async Task<bool> AccountNumberExists(
        string number,
        Guid? excludeAccountId,
        CancellationToken cancellationToken
    )
    {
        var all = await context.Accounts.ToListAsync(cancellationToken);
        return all.Any(a =>
            a.Number == number && (excludeAccountId is null || a.Id != excludeAccountId)
        );
    }

    public async Task<bool> AccountShortNameExists(
        string shortName,
        Guid? excludeAccountId,
        CancellationToken cancellationToken
    )
    {
        var all = await context.Accounts.ToListAsync(cancellationToken);
        return all.Any(a =>
            a.ShortName == shortName && (excludeAccountId is null || a.Id != excludeAccountId)
        );
    }

    public async Task<OrphanCleanupCounts> CleanupOrphanedAccountReferences(
        CancellationToken cancellationToken
    )
    {
        var existingAccountIds = (await context.Accounts.ToListAsync(cancellationToken))
            .Select(a => a.Id)
            .ToHashSet();

        var allJournalEntries = await context.JournalEntries.ToListAsync(cancellationToken);
        var orphanedJournalEntries = allJournalEntries
            .Where(e =>
                !existingAccountIds.Contains(
                    context.Entry(e).Property<Guid>("DebitAccountId").CurrentValue
                )
                || !existingAccountIds.Contains(
                    context.Entry(e).Property<Guid>("CreditAccountId").CurrentValue
                )
            )
            .ToList();
        context.JournalEntries.RemoveRange(orphanedJournalEntries);

        var allBudgetEntries = await context.BudgetEntries.ToListAsync(cancellationToken);
        var orphanedBudgetEntries = allBudgetEntries
            .Where(e =>
                !existingAccountIds.Contains(
                    context.Entry(e).Property<Guid>("AccountId").CurrentValue
                )
            )
            .ToList();
        context.BudgetEntries.RemoveRange(orphanedBudgetEntries);

        await context.SaveChangesAsync(cancellationToken);

        return new OrphanCleanupCounts(
            JournalEntriesDeleted: orphanedJournalEntries.Count,
            BudgetEntriesDeleted: orphanedBudgetEntries.Count
        );
    }
}
