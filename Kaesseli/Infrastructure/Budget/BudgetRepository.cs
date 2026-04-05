using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Budget;

public class BudgetRepository(KaesseliContext context) : IBudgetRepository
{
    public async Task<IEnumerable<BudgetEntry>> GetBudgetEntries(
        Guid accountingPeriodId, Guid? accountId, AccountType? accountType,
        CancellationToken cancellationToken)
    {
        var entries = await context.BudgetEntries
            .Where(e => EF.Property<Guid>(e, "AccountingPeriodId") == accountingPeriodId)
            .ToListAsync(cancellationToken);

        var neededAccountIds = entries
            .Select(e => context.Entry(e).Property<Guid>("AccountId").CurrentValue)
            .ToHashSet();
        var allCached = neededAccountIds.All(id => context.Accounts.Local.Any(a => a.Id == id));
        var accounts = allCached
            ? context.Accounts.Local.ToList()
            : await context.Accounts.ToListAsync(cancellationToken);
        var accountMap = accounts.ToDictionary(a => a.Id);

        var period = context.AccountingPeriods.Local.FirstOrDefault(p => p.Id == accountingPeriodId)
            ?? await context.AccountingPeriods.FirstOrDefaultAsync(p => p.Id == accountingPeriodId, cancellationToken);

        foreach (var entry in entries)
        {
            var accountFk = context.Entry(entry).Property<Guid>("AccountId").CurrentValue;
            if (accountMap.TryGetValue(accountFk, out var account))
                context.Entry(entry).Reference(e => e.Account).CurrentValue = account;
            if (period != null)
                context.Entry(entry).Reference(e => e.AccountingPeriod).CurrentValue = period;
        }

        IEnumerable<BudgetEntry> result = entries;
        if (accountId != null) result = result.Where(e => e.Account.Id == accountId);
        if (accountType is not null) result = result.Where(e => e.Account.Type == accountType);
        return result.ToList();
    }

    public async Task<BudgetEntry> SetBudget(BudgetEntry newBudgetEntryEntity, CancellationToken ct)
    {
        var newAccountId = newBudgetEntryEntity.Account.Id;
        var newPeriodId = newBudgetEntryEntity.AccountingPeriod.Id;

        var allEntries = await context.BudgetEntries
            .Where(b => EF.Property<Guid>(b, "AccountingPeriodId") == newPeriodId)
            .ToListAsync(ct);

        var currentEntry = allEntries
            .FirstOrDefault(b => context.Entry(b).Property<Guid>("AccountId").CurrentValue == newAccountId);

        if (currentEntry is null)
        {
            context.BudgetEntries.Add(newBudgetEntryEntity);
            await context.SaveChangesAsync(ct);
            return newBudgetEntryEntity;
        }

        currentEntry.Amount = newBudgetEntryEntity.Amount;
        currentEntry.Description = newBudgetEntryEntity.Description;
        await context.SaveChangesAsync(ct);
        return currentEntry;
    }
}
