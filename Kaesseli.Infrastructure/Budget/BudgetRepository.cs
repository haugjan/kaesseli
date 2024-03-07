using Kaesseli.Domain.Budget;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Budget;

public class BudgetRepository(KaesseliContext context) : IBudgetRepository
{
    public async Task<IEnumerable<BudgetEntry>> GetBudgetEntries(
        GetBudgetEntriesRequest request,
        CancellationToken cancellationToken)
    {
        var entries = context
                                                                      .BudgetEntries
                                                                      .Include(budget => budget.Account)
                                                                      .Include(budget => budget.AccountingPeriod)
                                                                      .Where(entry => entry.AccountingPeriod.Id == request.AccountingPeriodId);
        if (request.AccountId != null) entries = entries.Where(entry => entry.Account.Id == request.AccountId);
        if (request.AccountType is not null) entries = entries.Where(entry => entry.Account.Type == request.AccountType);
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