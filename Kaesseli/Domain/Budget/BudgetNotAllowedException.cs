using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Budget;

public class BudgetNotAllowedException : Exception
{
    public BudgetNotAllowedException(AccountType accountType) : base(
        message:
        $"Budget only allowed for '{nameof(AccountType.Revenue)}' or '{nameof(AccountType.Expense)}' but account type was '{accountType}'")
    {
    }
}