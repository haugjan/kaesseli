using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Budget;

public class GetBudgetEntriesRequest
{
    public required Guid? AccountingPeriodId { get; init; }
    public Guid? AccountId { get; init; }
    public AccountType? AccountType { get; init; }
    public static GetBudgetEntriesRequest Empty => new()
    {
        AccountingPeriodId = null
    };
}