using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

internal static class GetBudgetEntriesQueryExtensions
{
    internal static GetBudgetEntriesRequest ToGetBudgetEntriesRequest(this GetBudgetEntriesQuery query) =>
        new()
        {
            AccountId = query.AccountId,
            AccountingPeriodId = query.AccountingPeriodId,
            AccountType = query.AccountType
        };
}