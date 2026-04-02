using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Domain.Accounts;

public class AccountTests
{
    [Theory]
    [InlineData(AccountType.Asset, -2)]
    [InlineData(AccountType.Liability, 2)]
    [InlineData(AccountType.Revenue, 2)]
    [InlineData(AccountType.Expense, -2)]
    public void GetAccountBalance_ReturnsCorrectSign(
        AccountType accountType,
        decimal expectedBalance
    )
    {
        //Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Account",
            Type = accountType,
            Icon = new AccountIcon("favorite", "blue"),
        };
        var otherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other account",
            Type = AccountType.Revenue,
            Icon = new AccountIcon("favorite", "blue"),
        };
        var yetAnotherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other account",
            Type = AccountType.Expense,
            Icon = new AccountIcon("favorite", "blue"),
        };
        var entries = new List<JournalEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 3m,
                DebitAccount = account,
                CreditAccount = otherAccount,
                Transaction = null,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 5m,
                DebitAccount = yetAnotherAccount,
                CreditAccount = account,
                Transaction = null,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 42.42m,
                DebitAccount = otherAccount,
                CreditAccount = yetAnotherAccount,
                Transaction = null,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            },
        };

        //Act
        var accountBalance = AccountBalanceCalculator.GetAccountBalance(account, entries);

        //Assert
        accountBalance.ShouldBe(expectedBalance);
    }
}
