using Kaesseli.Features.Accounts;
using Kaesseli.Features.Journal;
using Xunit;

namespace Kaesseli.Test.Domain.Journal;

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
            Icon = new AccountIcon("favorite", "blue"),
        };
        var creditAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "B",
            Type = AccountType.Expense,
            Icon = new AccountIcon("favorite", "blue"),
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
            Transaction = null,
            AccountingPeriod = new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FromInclusive = default,
                ToInclusive = default,
                Description = string.Empty,
            },
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
            Icon = new AccountIcon("favorite", "blue"),
        };

        // Act & Assert
        Assert.Throws<AccountsMustNotBeSameException>(() =>
            new JournalEntry
            {
                Id = Guid.NewGuid(),
                ValueDate = DateOnly.FromDateTime(DateTime.Now),
                Description = "Test Description",
                Amount = 100m,
                DebitAccount = account,
                CreditAccount = account,
                Transaction = null,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            }
        );
    }
}
