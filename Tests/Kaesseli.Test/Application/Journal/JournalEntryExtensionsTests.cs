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
        var command = new AddJournalEntry.Query
        {
            Amount = 42,
            Description = "Description",
            ValueDate = null,
            CreditAccountId = creditAccount.Id,
            DebitAccountId = debitAccount.Id,
            AccountingPeriodId = Guid.NewGuid()
        };
        var valueDate = new DateOnly(year: 1982, month: 12, day: 13);
        var accountIngPeriod = new AccountingPeriod
        {
            Id = Guid.NewGuid(),
            Description = string.Empty,
            FromInclusive = default,
            ToInclusive = default
        };

        //Act
        var journalEntry = command.ToJournalEntry(valueDate: valueDate, debitAccount: debitAccount, creditAccount: creditAccount,
                                                  accountingPeriod: accountIngPeriod);

        //Assert
        journalEntry.Amount.Should().Be(command.Amount);
        journalEntry.Id.Should().NotBe(Guid.Empty);
        journalEntry.CreditAccount.Should().Be(creditAccount);
        journalEntry.DebitAccount.Should().Be(debitAccount);
        journalEntry.ValueDate.Should().Be(valueDate);
        journalEntry.Description.Should().Be(command.Description);
        journalEntry.AccountingPeriod.Should().Be(accountIngPeriod);
    }
}
