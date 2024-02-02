using Bogus;
using FluentAssertions;
using Kaesseli.Domain.Common;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Moq;

namespace Kaesseli.Domain.Test.Entities;

public class JournalEntryTests
{
    [Fact]
    public void SettingAccount_WhenAccountIsNull_ShouldNotThrowException()
    {
        // Arrange
        var journalEntry = CreateJournalEntry();
        var account = new Faker<Account>().UseSeed(seed: 0).Generate();

        // Act
        Action act = () => journalEntry.Account = account;

        // Assert
        act.Should().NotThrow<JournalEntriesImmutableException>();
    }

    [Fact]
    public void SettingAccount_WhenAccountIsAlreadySet_ShouldThrowJournalEntriesImmutableException()
    {
        // Arrange
        var journalEntry = CreateJournalEntry();
        var faker = new SmartFaker<Account>().UseSeed(seed: 1);
        var firstAccount = faker.Generate();
        var secondAccount = faker.Generate();
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
        var journalEntry = CreateJournalEntry();
        var account = new Faker<Account>().UseSeed(seed: 0).Generate();
        journalEntry.Account = account;

        // Act
        Action act = () => journalEntry.Account = account;

        // Assert
        act.Should().NotThrow();
    }

    private static JournalEntry CreateJournalEntry() =>
        new()
        {
            Id = new Guid(g: "{177D8117-459F-4962-B1BD-1CFE107A98AF}"),
            ValueDate = default,
            Description = "Description",
            Amount = 42.42m
        };
}