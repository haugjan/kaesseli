using Kaesseli.Features.Accounts;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Accounts;

public class AccountExtensionsTest
{
    [Fact]
    public void ToAccountSummary_ReturnsTransformed()
    {
        //Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Account",
            Type = AccountType.Expense,
            Icon = new AccountIcon("favorite", "blue"),
        };

        //Act
        var summary = account.ToAccountSummary(
            accountBalance: 3m,
            budget: 5m,
            budgetPerMonth: 7m,
            budgetPerYear: 8m,
            currentBudget: 100m,
            budgetBalance: 1200m
        );

        //Assert
        summary.Id.ShouldBe(account.Id);
        summary.Type.ShouldBe(account.Type.DisplayName());
        summary.Name.ShouldBe(account.Name);
        summary.AccountBalance.ShouldBe(3m);
        summary.Budget.ShouldBe(5m);
        summary.BudgetPerMonth.ShouldBe(7m);
        summary.BudgetPerYear.ShouldBe(8m);
        summary.CurrentBudget.ShouldBe(100m);
        summary.BudgetBalance.ShouldBe(1200m);
    }
}
