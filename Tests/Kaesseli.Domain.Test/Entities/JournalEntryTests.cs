using FluentAssertions;
using Kaesseli.Domain.Common;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Domain.Test.Entities;

public class JournalEntryTests
{
    [Fact]
    public void SettingAccount_WhenAccountIsNull_ShouldNotThrowException()
    {
        // Arrange
        var journalEntry = new JournalEntry();
        var account = new Account();

        // Act
        Action act = () => journalEntry.Account = account;

        // Assert
        act.Should().NotThrow<JournalEntriesImmutableException>();
    }

    [Fact]
    public void SettingAccount_WhenAccountIsAlreadySet_ShouldThrowJournalEntriesImmutableException()
    {
        // Arrange
        var journalEntry = new JournalEntry();
        var firstAccount = new Account();
        var secondAccount = new Account();
        journalEntry.Account = firstAccount;

        // Act
        Action act = () => journalEntry.Account = secondAccount;

        // Assert
        act.Should().Throw<JournalEntriesImmutableException>();
    }

    [Fact]
    public void SettingAccount_WhenSettingSameAccountAgain_ShouldNotThrowException()
    {
        // Arrange
        var journalEntry = new JournalEntry();
        var account = new Account();
        journalEntry.Account = account;

        // Act
        Action act = () => journalEntry.Account = account;

        // Assert
        act.Should().NotThrow<JournalEntriesImmutableException>();
    }
}