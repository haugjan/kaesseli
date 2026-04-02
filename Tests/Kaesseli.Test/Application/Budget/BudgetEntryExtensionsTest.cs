using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Budget;

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

        //Act
        var queryResult = budgetEntry.ToGetBudgetEntriesQueryResult();

        //Assert
        queryResult.AccountId.ShouldBe(budgetEntry.Account.Id);
        queryResult.AccountingPeriodId.ShouldBe(budgetEntry.AccountingPeriod.Id);
        queryResult.Id.ShouldBe(budgetEntry.Id);
        queryResult.Description.ShouldBe(budgetEntry.Description);
        queryResult.Amount.ShouldBe(budgetEntry.Amount);
    }
}
