using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class BudgetEntryExtensionsTest
{
    [Fact]
    public void ToGetBudgetEntriesQueryResult_ReturnsCorrectQueryResult()
    {
        //Arrange
        var budgetEntry = new BudgetEntry
        {
            Id = Guid.NewGuid(),
            Description = "Description",
            Amount = 42,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Name",
                Type = AccountType.Expense,
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

        //Act
        var queryResult = budgetEntry.ToGetBudgetEntriesQueryResult();

        //Assert
        queryResult.Should().BeEquivalentTo(budgetEntry, options => options.Excluding(be => be.Account).Excluding(be=> be.AccountingPeriod));
        queryResult.AccountId.Should().Be(budgetEntry.Account.Id);
        queryResult.AccountingPeriodId.Should().Be(budgetEntry.AccountingPeriod.Id);
    }
}