namespace Kaesseli.Domain.Budget;

public interface IBudgetRepository
{
    Task<BudgetEntry> SetBudget(BudgetEntry newBudgetEntryEntity, CancellationToken ct);

    Task<IEnumerable<BudgetEntry>> GetBudgetEntries(
        GetBudgetEntriesRequest request,
        CancellationToken cancellationToken);
}