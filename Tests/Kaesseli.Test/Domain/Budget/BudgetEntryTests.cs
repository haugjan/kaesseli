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
            new BudgetEntry
            {
                Id = Guid.NewGuid(),
                Description = "Budget entry",
                Amount = 42.42m,
                Account = new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Account",
                    Type = accountType,
                    Icon = new AccountIcon("favorite", "blue"),
                },
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            };

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
            new BudgetEntry
            {
                Id = Guid.NewGuid(),
                Description = "Budget entry",
                Amount = 42.42m,
                Account = new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Account",
                    Type = accountType,
                    Icon = new AccountIcon("favorite", "blue"),
                },
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            };

        //Assert
        Should.NotThrow(makeNewBudgetEntry);
    }
}
