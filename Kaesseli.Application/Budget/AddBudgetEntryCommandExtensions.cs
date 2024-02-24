using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

internal static class AddBudgetEntryCommandExtensions
{
    internal static BudgetEntry ToBudgetEntry(this AddBudgetEntryCommand budgetEntryCommand, Account account, DateOnly valueDate) =>
        new()
        {
            Id = Guid.NewGuid(),
            Amount = budgetEntryCommand.Amount,
            Description = budgetEntryCommand.Description,
            Account = account,
            ValueDate = valueDate
        };
}