using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using Xunit;

namespace Kaesseli.Domain.Test.Accounts;

public class AccountTests
{
    [Theory]
    [InlineData(AccountType.Asset, -2)]
    [InlineData(AccountType.Liability, 2)]
    [InlineData(AccountType.Revenue, 2)]
    [InlineData(AccountType.Expense, -2)]
    public void GetAccountBalance_ReturnsCorrectSign(AccountType accountType, decimal expectedBalance)
    {
        //Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Account",
            Type = accountType,
            Icon = "favorite",
            IconColor = "blue"
        };
        var otherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other account",
            Type = AccountType.Revenue,
            Icon = "favorite",
            IconColor = "blue"
        };
        var yetAnotherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other account",
            Type = AccountType.Expense,
            Icon = "favorite",
            IconColor = "blue"
        };
        var entries = new List<JournalEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(
                    year: 2000,
                    month: 12,
                    day: 13),
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
                    Description = string.Empty
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(
                    year: 2000,
                    month: 12,
                    day: 13),
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
                    Description = string.Empty
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(
                    year: 2000,
                    month: 12,
                    day: 13),
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
                    Description = string.Empty
                }
            }
        };

        //Act
        var accountBalance = account.GetAccountBalance(entries);

        //Assert
        accountBalance.Should().Be(expectedBalance);
    }

    [Fact]
    public void GetBudget_ReturnCorrectBudget()
    {
        //Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Account",
            Type = AccountType.Revenue,
            Icon = "favorite",
            IconColor = "blue"
        };
        var otherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Account",
            Type = AccountType.Revenue,
            Icon = "favorite",
            IconColor = "blue"
        };
        var budgetEntries = new List<BudgetEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Description",
                Amount = 3,
                Account = account,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Description",
                Amount = 5,
                Account = otherAccount,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Description",
                Amount = 7,
                Account = account,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = default,
                    Description = string.Empty,
                    FromInclusive = default,
                    ToInclusive = default
                }
            }
        };
        //Act
        var budget = Account.GetBudget(budgetEntries);

        //Assert
        budget.Should().Be(expected: 10);
    }
}