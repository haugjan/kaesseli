
// ReSharper disable once CheckNamespace
namespace Kaesseli.Features.Budget;

internal static class BudgetEntryExtensions
{
    internal static GetBudgetEntries.Result ToGetBudgetEntriesQueryResult(this BudgetEntry budgetEntry) =>
        new(
            Id: budgetEntry.Id,
            Amount: budgetEntry.Amount,
            Description: budgetEntry.Description,
            AccountId: budgetEntry.Account.Id,
            AccountingPeriodId: budgetEntry.AccountingPeriod.Id);
}
