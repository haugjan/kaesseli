using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using Kaesseli.Test.Faker;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Journal;

public class JournalEntryExtensionsTests
{
    [Fact]
    public void ToJournalEntry_ReturnsJournalEntry()
    {
        //Arrange
        var creditAccount = new SmartFaker<Account>().Generate();
        var debitAccount = new SmartFaker<Account>().Generate();
        var command = new AddJournalEntry.Query(
            Amount: 42,
            Description: "Description",
            ValueDate: null,
            DebitAccountId: debitAccount.Id,
            CreditAccountId: creditAccount.Id,
            AccountingPeriodId: Guid.NewGuid());
        var valueDate = new DateOnly(year: 1982, month: 12, day: 13);
        var accountIngPeriod = new AccountingPeriod
        {
            Id = Guid.NewGuid(),
            Description = string.Empty,
            FromInclusive = default,
            ToInclusive = default,
        };

        //Act
        var journalEntry = command.ToJournalEntry(
            valueDate: valueDate,
            debitAccount: debitAccount,
            creditAccount: creditAccount,
            accountingPeriod: accountIngPeriod
        );

        //Assert
        journalEntry.Amount.ShouldBe(command.Amount);
        journalEntry.Id.ShouldNotBe(Guid.Empty);
        journalEntry.CreditAccount.ShouldBe(creditAccount);
        journalEntry.DebitAccount.ShouldBe(debitAccount);
        journalEntry.ValueDate.ShouldBe(valueDate);
        journalEntry.Description.ShouldBe(command.Description);
        journalEntry.AccountingPeriod.ShouldBe(accountIngPeriod);
    }
}
