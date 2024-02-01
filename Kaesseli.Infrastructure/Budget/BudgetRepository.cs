using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;

namespace Kaesseli.Infrastructure.Budget;

public class BudgetRepository : IBudgetRepository
{
    private readonly BudgetContext _context;

    public BudgetRepository(BudgetContext context) =>
        _context = context;

    public async Task<Account> GetAccount(Guid accountId, CancellationToken ct) =>
        await _context.Accounts.FindAsync(accountId, ct)
     ?? throw new AccountNotFoundException(accountId);

    public async Task<BudgetEntry> AddBudgetEntry(BudgetEntry newBudgetEntryEntity, CancellationToken ct)
    {
        _context.BudgetEntries.Add(newBudgetEntryEntity);
        await _context.SaveChangesAsync(ct);
        return newBudgetEntryEntity;
    }

    public async Task AssignAccount(Guid budgetId, Guid accountId, CancellationToken ct)
    {
        var budgetEntry = _context.BudgetEntries.Single(entry => entry.Id == budgetId);
        budgetEntry.Account = _context.Accounts.Single(account => account.Id == accountId);
        await _context.SaveChangesAsync(ct);
    }
}