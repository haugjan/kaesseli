using FluentAssertions;
using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using Kaesseli.TestUtilities.Faker;
using Xunit;

namespace Kaesseli.Application.Test.Journal;

public class JournalEntryExtensionsTests
{
    [Fact]
    public void ToJournalEntry_ReturnsJournalEntry()
    {
        //Arrange
        var creditAccount = new SmartFaker<Account>().Generate();
        var debitAccount = new SmartFaker<Account>().Generate();
        var command = new AddJournalEntryCommand
        {
            Amount = 42,
            Description = "Description",
            ValueDate = null,
            CreditAccountId = creditAccount.Id,
            DebitAccountId = debitAccount.Id
        };
        var valueDate = new DateOnly(year: 1982, month: 12, day: 13);

        //Act
        var journalEntry = command.ToJournalEntry(valueDate: valueDate, debitAccount, creditAccount);

        //Assert
        journalEntry.Amount.Should().Be(command.Amount);
        journalEntry.Id.Should().NotBe(Guid.Empty);
        journalEntry.CreditAccount.Should().Be(creditAccount);
        journalEntry.DebitAccount.Should().Be(debitAccount);
        journalEntry.ValueDate.Should().Be(valueDate);
        journalEntry.Description.Should().Be(command.Description);
    }
}