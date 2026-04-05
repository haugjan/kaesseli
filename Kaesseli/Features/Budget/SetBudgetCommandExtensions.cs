using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Budget;

internal static class SetBudgetCommandExtensions
{
    internal static BudgetEntry ToBudgetEntry(
        this SetBudget.Query budgetCommand,
        Account account,
        AccountingPeriod accountingPeriod) =>
        BudgetEntry.Create(budgetCommand.Description, budgetCommand.Amount, account, accountingPeriod);
}
