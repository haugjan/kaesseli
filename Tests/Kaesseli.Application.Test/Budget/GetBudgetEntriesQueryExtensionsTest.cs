using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.Domain.Accounts;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class GetBudgetEntriesQueryExtensionsTest
{
    [Fact]
    public void ToGetBudgetEntriesRequest_ReturnsGetBudgetEntriesRequest()
    {
        //Arrange
        var budgetEntriesQuery = new GetBudgetEntriesQuery
        {
            AccountId = Guid.NewGuid(),
            FromDate = new DateOnly(year: 1982, month: 11, day: 3),
            ToDate = new DateOnly(year: 2000, month: 12, day: 13),
            AccountType = AccountType.Asset
        };

        //Act
        var entriesRequest = budgetEntriesQuery.ToGetBudgetEntriesRequest();

        //Assert
        entriesRequest.Should().BeEquivalentTo(budgetEntriesQuery);
    }
}