using Kaesseli.Domain.Common;
using Kaesseli.Domain.Journal;
using Xunit;

namespace Kaesseli.Domain.Test.Journal;

public class JournalEntryTests
{
    [Fact]
    public void CreatingJournalEntry_WithDifferentDebitAndCreditAccount_ShouldSucceed()
    {
        // Arrange
        var debitAccount = new Account { Id = Guid.NewGuid(), Name = "A", Type = AccountType.Expense };
        var creditAccount = new Account { Id = Guid.NewGuid(), Name = "B", Type = AccountType.Expense };

        // Act & Assert
        _ = new JournalEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = DateOnly.FromDateTime(DateTime.Now),
            Description = "Test Description",
            Amount = 100m,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount
        };
    }

    [Fact]
    public void CreatingJournalEntry_WithIdenticalDebitAndCreditAccount_ShouldThrowException()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "A", Type = AccountType.Expense };

        // Act & Assert
        Assert.Throws<AccountsMustNotBeSameException>(
            () => new JournalEntry
            {
                Id = Guid.NewGuid(),
                ValueDate = DateOnly.FromDateTime(DateTime.Now),
                Description = "Test Description",
                Amount = 100m,
                DebitAccount = account,
                CreditAccount = account
            });
    }
}