
// ReSharper disable once CheckNamespace
namespace Kaesseli.Features.Budget;

internal static class BudgetEntryExtensions
{
    extension(BudgetEntry budgetEntry)
    {
        internal GetBudgetEntries.Result ToGetBudgetEntriesQueryResult() =>
            new(
                Id: budgetEntry.Id,
                Amount: budgetEntry.Amount,
                Description: budgetEntry.Description,
                AccountId: budgetEntry.Account.Id,
                AccountingPeriodId: budgetEntry.AccountingPeriod.Id);
    }
}
