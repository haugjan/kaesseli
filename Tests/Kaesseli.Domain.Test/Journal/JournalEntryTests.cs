using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Xunit;

namespace Kaesseli.Domain.Test.Journal;

public class JournalEntryTests
{
    [Fact]
    public void CreatingJournalEntry_WithDifferentDebitAndCreditAccount_ShouldSucceed()
    {
        // Arrange
        var debitAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "A",
            Type = AccountType.Expense,
            Icon = "favorite",
            IconColor = "blue"
        };
        var creditAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "B",
            Type = AccountType.Expense,
            Icon = "favorite",
            IconColor = "blue"
        };

        // Act & Assert
        _ = new JournalEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = DateOnly.FromDateTime(DateTime.Now),
            Description = "Test Description",
            Amount = 100m,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            Transaction = null
        };
    }

    [Fact]
    public void CreatingJournalEntry_WithIdenticalDebitAndCreditAccount_ShouldThrowException()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "A",
            Type = AccountType.Expense,
            Icon = "favorite",
            IconColor = "blue"
        };

        // Act & Assert
        Assert.Throws<AccountsMustNotBeSameException>(
            () => new JournalEntry
            {
                Id = Guid.NewGuid(),
                ValueDate = DateOnly.FromDateTime(DateTime.Now),
                Description = "Test Description",
                Amount = 100m,
                DebitAccount = account,
                CreditAccount = account,
                Transaction = null
            });
    }
}