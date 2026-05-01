using Kaesseli.Features.Accounts;
using Kaesseli.Features.Journal;
using Xunit;

namespace Kaesseli.Test.Features.Journal;

public class JournalEntryTests
{
    [Fact]
    public void CreatingJournalEntry_WithDifferentDebitAndCreditAccount_ShouldSucceed()
    {
        // Arrange
        var debitAccount = AccountFactory.Create(
            "A",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );
        var creditAccount = AccountFactory.Create(
            "B",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );

        // Act & Assert
        _ = JournalEntry.Create(
            valueDate: DateOnly.FromDateTime(DateTime.Now),
            description: "Test Description",
            amount: 100m,
            debitAccount: debitAccount,
            creditAccount: creditAccount,
            accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
        );
    }

    [Fact]
    public void CreatingJournalEntry_WithIdenticalDebitAndCreditAccount_ShouldThrowException()
    {
        // Arrange
        var account = AccountFactory.Create(
            "A",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );

        // Act & Assert
        Assert.Throws<AccountsMustNotBeSameException>(() =>
            JournalEntry.Create(
                valueDate: DateOnly.FromDateTime(DateTime.Now),
                description: "Test Description",
                amount: 100m,
                debitAccount: account,
                creditAccount: account,
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            )
        );
    }
}
