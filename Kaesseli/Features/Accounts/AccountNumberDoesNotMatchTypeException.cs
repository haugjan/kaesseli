namespace Kaesseli.Features.Accounts;

public class AccountNumberDoesNotMatchTypeException(string number, AccountType type)
    : Exception(
        $"Account number '{number}' does not match account type {type}. "
            + "Expected first digit: Asset=1, Liability=2, Revenue=3, Expense=4."
    );
