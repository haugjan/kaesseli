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
            ValueDate = new DateOnly(year: 1982, month: 11, day: 3),
            Description = "Description",
            Amount = 42,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Name",
                Type = AccountType.Expense,
                Icon = "favorite",
                IconColor = "blue"
            }
        };

        //Act
        var queryResult = budgetEntry.ToGetBudgetEntriesQueryResult();

        //Assert
        queryResult.Should().BeEquivalentTo(budgetEntry, options => options.Excluding(be => be.Account));
        queryResult.AccountId.Should().Be(budgetEntry.Account.Id);
    }
}