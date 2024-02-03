using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Budget;

public class BudgetRepository(KaesseliContext context) : IBudgetRepository
{

    public async Task<Account> GetAccount(Guid accountId, CancellationToken ct) =>
        await context.Accounts.FindAsync(accountId, ct)
     ?? throw new AccountNotFoundException(accountId);

    public async Task<IEnumerable<BudgetEntry>> GetBudgetEntries(
        GetBudgetEntriesRequest request,
        CancellationToken cancellationToken)
    {
        IQueryable<BudgetEntry> entries = context.BudgetEntries.Include(budget=> budget.Account);
        if(request.AccountId  != null)
            entries = entries.Where(entry=> entry.Account.Id==request.AccountId);
        if (request.FromDate is not null) 
            entries = entries.Where(entry => entry.ValueDate >= request.FromDate);
        if (request.ToDate is not null)
            entries = entries.Where(entry => entry.ValueDate < request.ToDate);

        return await entries.ToListAsync(cancellationToken);
    }



    public async Task<BudgetEntry> AddBudgetEntry(BudgetEntry newBudgetEntryEntity, CancellationToken ct)
    {
        context.BudgetEntries.Add(newBudgetEntryEntity);
        await context.SaveChangesAsync(ct);
        return newBudgetEntryEntity;
    }


}