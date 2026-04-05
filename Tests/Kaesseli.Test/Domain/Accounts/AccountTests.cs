using Kaesseli.Features.Accounts;
using Kaesseli.Features.Journal;
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
        var account = Account.Create("Account", accountType, new AccountIcon("favorite", "blue"));
        var otherAccount = Account.Create("Other account", AccountType.Revenue, new AccountIcon("favorite", "blue"));
        var yetAnotherAccount = Account.Create("Other account", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);
        var entries = new List<JournalEntry>
        {
            JournalEntry.Create(
                valueDate: new DateOnly(year: 2000, month: 12, day: 13),
                description: "Description",
                amount: 3m,
                debitAccount: account,
                creditAccount: otherAccount,
                accountingPeriod: accountingPeriod
            ),
            JournalEntry.Create(
                valueDate: new DateOnly(year: 2000, month: 12, day: 13),
                description: "Description",
                amount: 5m,
                debitAccount: yetAnotherAccount,
                creditAccount: account,
                accountingPeriod: accountingPeriod
            ),
            JournalEntry.Create(
                valueDate: new DateOnly(year: 2000, month: 12, day: 13),
                description: "Description",
                amount: 42.42m,
                debitAccount: otherAccount,
                creditAccount: yetAnotherAccount,
                accountingPeriod: accountingPeriod
            ),
        };

        //Act
        var accountBalance = AccountBalanceCalculator.GetAccountBalance(account, entries);

        //Assert
        accountBalance.ShouldBe(expectedBalance);
    }
}
