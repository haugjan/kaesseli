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
            ValueDate = new DateOnly(year: 1982, month: 11, day: 3)
        };

        //Act
        var budgetEntry = budgetEntryCommand.ToBudgetEntry(account, budgetEntryCommand.ValueDate.Value);

        //Assert
        budgetEntry.Account.Should().Be(account);
        budgetEntry.Amount.Should().Be(budgetEntryCommand.Amount);
        budgetEntry.Description.Should().Be(budgetEntryCommand.Description);
        budgetEntry.Id.Should().NotBe(Guid.Empty);
        budgetEntry.ValueDate.Should().Be(budgetEntryCommand.ValueDate);
    }
}