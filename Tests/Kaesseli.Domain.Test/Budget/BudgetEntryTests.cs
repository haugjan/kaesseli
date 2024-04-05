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
            Description = "Budget entry",
            Amount = 42.42m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account",
                Type = accountType,
                Icon = "favorite",
                IconColor = "blue"
            },
            AccountingPeriod = new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FromInclusive = default,
                ToInclusive = default,
                Description = string.Empty
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
            Description = "Budget entry",
            Amount = 42.42m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account",
                Type = accountType,
                Icon = "favorite",
                IconColor = "blue"
            },
            AccountingPeriod = new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FromInclusive = default,
                ToInclusive = default,
                Description = string.Empty
            }
        };

        //Assert
        makeNewBudgetEntry.Should().NotThrow();
    }
}