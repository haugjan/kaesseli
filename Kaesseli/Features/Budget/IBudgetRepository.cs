using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Budget;

public interface IBudgetRepository
{
    Task<BudgetEntry> SetBudget(BudgetEntry newBudgetEntryEntity, CancellationToken ct);

    Task<IEnumerable<BudgetEntry>> GetBudgetEntries(
        Guid accountingPeriodId, Guid? accountId, AccountType? accountType,
        CancellationToken cancellationToken);
}
