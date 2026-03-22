using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQuery
{
    public required Guid? AccountId { get; init; }
    public required AccountType? AccountType { get; init; }
    public required Guid AccountingPeriodId { get; init; }
}