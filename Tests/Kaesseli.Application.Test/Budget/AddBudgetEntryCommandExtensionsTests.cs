using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.Domain.Accounts;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class AddBudgetEntryCommandExtensionsTests
{
    [Fact]
    public void ToBudgetEntry_ReturnBudgetEntry()
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
        var budgetEntryCommand = new AddBudgetEntryCommand
        {
            Amount = 42,
            Description = "Description",
            AccountId = Guid.NewGuid(),
            AccountingPeriodId = Guid.NewGuid()
        };
        var accountingPeriod = new AccountingPeriod
        {
            Id = Guid.NewGuid(),
            Description = string.Empty,
            FromInclusive = default,
            ToInclusive = default
        };

        //Act
        var budgetEntry = budgetEntryCommand.ToBudgetEntry(account, accountingPeriod);

        //Assert
        budgetEntry.Account.Should().Be(account);
        budgetEntry.Amount.Should().Be(budgetEntryCommand.Amount);
        budgetEntry.Description.Should().Be(budgetEntryCommand.Description);
        budgetEntry.Id.Should().NotBe(Guid.Empty);
        budgetEntry.AccountingPeriod.Should().Be(accountingPeriod);
    }
}