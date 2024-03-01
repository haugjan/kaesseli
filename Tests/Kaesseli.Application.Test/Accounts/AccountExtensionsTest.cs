using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Xunit;

namespace Kaesseli.Application.Test.Accounts;

public class AccountExtensionsTest
{
    [Fact]
    public void Test()
    {
        //Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Account",
            Type = AccountType.Expense,
            Icon = "favorite",
            IconColor = "blue"
        };

        //Act
        var summary = account.ToAccountSummary(accountBalance: 3m, budget: 5m, budgetBalance: 7m);

        //Assert
        summary.Id.Should().Be(account.Id);
        summary.Type.Should().Be(expected: account.Type.DisplayName());
        summary.Name.Should().Be(account.Name);
        summary.AccountBalance.Should().Be(expected: 3m);
        summary.Budget.Should().Be(expected: 5m);
        summary.BudgetBalance.Should().Be(expected: 7m);
    }
}