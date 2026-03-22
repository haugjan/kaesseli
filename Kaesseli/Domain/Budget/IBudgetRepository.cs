using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Budget;

public interface IBudgetRepository
{
    Task<BudgetEntry> SetBudget(BudgetEntry newBudgetEntryEntity, CancellationToken ct);

    Task<IEnumerable<BudgetEntry>> GetBudgetEntries(
        Guid accountingPeriodId, Guid? accountId, AccountType? accountType,
        CancellationToken cancellationToken);
}