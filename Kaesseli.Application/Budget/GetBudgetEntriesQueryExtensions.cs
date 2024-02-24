using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

internal static class GetBudgetEntriesQueryExtensions
{
    internal static GetBudgetEntriesRequest ToGetBudgetEntriesRequest(this GetBudgetEntriesQuery query) =>
        new()
        {
            AccountId = query.AccountId,
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            AccountType = query.AccountType
        };
}