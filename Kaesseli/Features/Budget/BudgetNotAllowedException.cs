using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Budget;

public class BudgetNotAllowedException : Exception
{
    public BudgetNotAllowedException(AccountType accountType) : base(
        message:
        $"Budget only allowed for '{nameof(AccountType.Revenue)}' or '{nameof(AccountType.Expense)}' but account type was '{accountType}'")
    {
    }
}
