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
    [InlineData(AccountType.Revenue, -2)]
    [InlineData(AccountType.Expense, 2)]
    public void GetAccountBalance_ReturnsCorrectSign(AccountType accountType, decimal expectedBalance)
    {
        //Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "Account", Type = accountType };
        var otherAccount = new Account { Id = Guid.NewGuid(), Name = "Other account", Type = AccountType.Revenue };
        var yetAnotherAccount = new Account { Id = Guid.NewGuid(), Name = "Other account", Type = AccountType.Expense };
        var entries = new List<JournalEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 3m,
                DebitAccount = account,
                CreditAccount = otherAccount
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 5m,
                DebitAccount = yetAnotherAccount,
                CreditAccount = account
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 42.42m,
                DebitAccount = otherAccount,
                CreditAccount = yetAnotherAccount
            }
        };

        //Act
        var accountBalance = account.GetAccountBalance(entries);

        //Assert
        accountBalance.Should().Be(expectedBalance);
    }

    public void Test()
    {
        //Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "Account", Type = AccountType.Revenue };
        var otherAccount = new Account { Id = Guid.NewGuid(), Name = "Account", Type = AccountType.Revenue };
        var budgetEntries = new List<BudgetEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 3,
                Account = account
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 5,
                Account = otherAccount
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
                Description = "Description",
                Amount = 7,
                Account = account
            }
        };
        //Act
        var budget = account.GetBudget(budgetEntries);

        //Assert
        budget.Should().Be(expected: 10);
    }
}