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
            AccountType = AccountType.Asset,
            AccountingPeriodId = Guid.NewGuid()
        };

        //Act
        var entriesRequest = budgetEntriesQuery.ToGetBudgetEntriesRequest();

        //Assert
        entriesRequest.Should().BeEquivalentTo(budgetEntriesQuery);
    }
}