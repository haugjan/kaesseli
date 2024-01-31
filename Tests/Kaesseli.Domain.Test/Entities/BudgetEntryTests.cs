using FluentAssertions;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Domain.Test.Entities;

public class BudgetEntryTests
{
    [Fact]
    public void SettingAccount_WhenAccountIsNull_ShouldNotThrowException()
    {
        // Arrange
        var budgetEntry = new BudgetEntry();
        var account = new Account();

        // Act
        Action act = () => budgetEntry.Account = account;

        // Assert
        act.Should().NotThrow<JournalEntriesImmutableException>();
    }

    [Fact]
    public void SettingAccount_WhenAccountIsAlreadySet_ShouldThrowJournalEntriesImmutableException()
    {
        // Arrange
        var budgetEntry = new BudgetEntry();
        var firstAccount = new Account();
        var secondAccount = new Account();
        budgetEntry.Account = firstAccount;

        // Act
        Action act = () => budgetEntry.Account = secondAccount;

        // Assert
        act.Should().Throw<JournalEntriesImmutableException>();
    }

    [Fact]
    public void SettingAccount_WhenSettingSameAccountAgain_ShouldNotThrowException()
    {
        // Arrange
        var budgetEntry = new BudgetEntry();
        var account = new Account();
        budgetEntry.Account = account;

        // Act
        Action act = () => budgetEntry.Account = account;

        // Assert
        act.Should().NotThrow<JournalEntriesImmutableException>();
    }
}