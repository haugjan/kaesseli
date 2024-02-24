using Kaesseli.Application.Budget;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Budget;

internal static class BudgetEntryExtensions
{
    internal static GetBudgetEntriesQueryResult ToGetBudgetEntriesQueryResult(this BudgetEntry budgetEntry) =>
        new()
        {
            Id = budgetEntry.Id,
            Amount = budgetEntry.Amount,
            Description = budgetEntry.Description,
            AccountId = budgetEntry.Account.Id,
            ValueDate = budgetEntry.ValueDate
        };
}