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
        IQueryable<BudgetEntry> entries = context.BudgetEntries.Include(budget => budget.Account);
        if (request.AccountId != null) entries = entries.Where(entry => entry.Account.Id == request.AccountId);
        if (request.FromDate is not null) entries = entries.Where(entry => entry.ValueDate >= request.FromDate);
        if (request.ToDate is not null) entries = entries.Where(entry => entry.ValueDate < request.ToDate);
        if (request.AccountType is not null) entries = entries.Where(entry => entry.Account.Type == request.AccountType);
        return await entries.ToListAsync(cancellationToken);
    }

    public async Task<BudgetEntry> AddBudgetEntry(BudgetEntry newBudgetEntryEntity, CancellationToken ct)
    {
        context.BudgetEntries.Add(newBudgetEntryEntity);
        await context.SaveChangesAsync(ct);
        return newBudgetEntryEntity;
    }
}