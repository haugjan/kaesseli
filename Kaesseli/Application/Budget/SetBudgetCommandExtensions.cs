using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

internal static class SetBudgetCommandExtensions
{
    internal static BudgetEntry ToBudgetEntry(
        this SetBudgetCommand budgetCommand,
        Account account,
        AccountingPeriod accountingPeriod) =>
        new()
        {
            Id = Guid.NewGuid(),
            Amount = budgetCommand.Amount,
            Description = budgetCommand.Description,
            Account = account,
            AccountingPeriod = accountingPeriod
        };
}