using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Domain.Budget;

public class BudgetEntryTests
{
    [Theory]
    [InlineData(AccountType.Asset)]
    [InlineData(AccountType.Liability)]
    public void SetAccount_WrongBudgetType_ThrowsException(AccountType accountType)
    {
        //Arrange & Act
        var makeNewBudgetEntry = () =>
            BudgetEntry.Create(
                description: "Budget entry",
                amount: 42.42m,
                account: Account.Create("Account", accountType, new AccountIcon("favorite", "blue")),
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            );

        //Assert
        Should.Throw<BudgetNotAllowedException>(makeNewBudgetEntry);
    }

    [Theory]
    [InlineData(AccountType.Revenue)]
    [InlineData(AccountType.Expense)]
    public void SetAccount_CorrectBudgetType_ThrowsException(AccountType accountType)
    {
        //Arrange & Act
        var makeNewBudgetEntry = () =>
            BudgetEntry.Create(
                description: "Budget entry",
                amount: 42.42m,
                account: Account.Create("Account", accountType, new AccountIcon("favorite", "blue")),
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            );

        //Assert
        Should.NotThrow(makeNewBudgetEntry);
    }
}
