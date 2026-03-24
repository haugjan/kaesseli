using FluentAssertions;
using Kaesseli.Domain.Accounts;
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
        summary.Id.Should().Be(account.Id);
        summary.Type.Should().Be(expected: account.Type.DisplayName());
        summary.Name.Should().Be(account.Name);
        summary.AccountBalance.Should().Be(expected: 3m);
        summary.Budget.Should().Be(expected: 5m);
        summary.CurrentBudget.Should().Be(expected: 7m);
        summary.BudgetBalance.Should().Be(expected: 8m);
    }
}
