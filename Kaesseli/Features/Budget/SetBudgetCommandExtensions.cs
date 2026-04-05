using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Budget;

internal static class SetBudgetCommandExtensions
{
    extension(SetBudget.Query budgetCommand)
    {
        internal BudgetEntry ToBudgetEntry(
            Account account,
            AccountingPeriod accountingPeriod) =>
            BudgetEntry.Create(budgetCommand.Description, budgetCommand.Amount, account, accountingPeriod);
    }
}
