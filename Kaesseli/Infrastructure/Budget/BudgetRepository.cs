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
        var entries = context
                                                                      .BudgetEntries
                                                                      .Include(budget => budget.Account)
                                                                      .Include(budget => budget.AccountingPeriod)
                                                                      .Where(entry => entry.AccountingPeriod.Id == accountingPeriodId);
        if (accountId != null) entries = entries.Where(entry => entry.Account.Id == accountId);
        if (accountType is not null) entries = entries.Where(entry => entry.Account.Type == accountType);
        return await entries.ToListAsync(cancellationToken);
    }

    public async Task<BudgetEntry> SetBudget(BudgetEntry newBudgetEntryEntity, CancellationToken ct)
    {
        var currentEntry = await context.BudgetEntries
                                        .Where(budget => budget.Account.Id == newBudgetEntryEntity.Account.Id)
                                        .Where(budget => budget.AccountingPeriod.Id == newBudgetEntryEntity.AccountingPeriod.Id)
                                        .FirstOrDefaultAsync(ct);
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