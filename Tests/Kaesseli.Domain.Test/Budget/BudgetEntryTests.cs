using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Xunit;

namespace Kaesseli.Domain.Test.Budget;

public class BudgetEntryTests
{
    [Theory]
    [InlineData(AccountType.Asset)]
    [InlineData(AccountType.Liability)]
    public void SetAccount_WrongBudgetType_ThrowsException(AccountType accountType)
    {
        //Arrange & Act
        var makeNewBudgetEntry = () => new BudgetEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
            Description = "Budget entry",
            Amount = 42.42m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account",
                Type = accountType,
                Icon = "favorite",
                IconColor = "blue"
            }
        };

        //Assert
        makeNewBudgetEntry.Should().ThrowExactly<BudgetNotAllowedException>();
    }

    [Theory]
    [InlineData(AccountType.Revenue)]
    [InlineData(AccountType.Expense)]
    public void SetAccount_CorrectBudgetType_ThrowsException(AccountType accountType)
    {
        //Arrange & Act
        var makeNewBudgetEntry = () => new BudgetEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
            Description = "Budget entry",
            Amount = 42.42m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account",
                Type = accountType,
                Icon = "favorite",
                IconColor = "blue"
            }
        };

        //Assert
        makeNewBudgetEntry.Should().NotThrow();
    }
}