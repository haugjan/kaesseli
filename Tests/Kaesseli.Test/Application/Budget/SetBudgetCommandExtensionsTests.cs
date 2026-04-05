using Kaesseli.Features.Budget;
using Kaesseli.Features.Accounts;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Budget;

public class SetBudgetCommandExtensionsTests
{
    [Fact]
    public void ToBudgetEntry_ReturnBudgetEntry()
    {
        //Arrange
        var account = Account.Create("Account", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var budgetEntryCommand = new SetBudget.Query(Amount: 42, Description: "Description", AccountId: Guid.NewGuid(), AccountingPeriodId: Guid.NewGuid());
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);

        //Act
        var budgetEntry = budgetEntryCommand.ToBudgetEntry(account, accountingPeriod);

        //Assert
        budgetEntry.Account.ShouldBe(account);
        budgetEntry.Amount.ShouldBe(budgetEntryCommand.Amount);
        budgetEntry.Description.ShouldBe(budgetEntryCommand.Description);
        budgetEntry.Id.ShouldNotBe(Guid.Empty);
        budgetEntry.AccountingPeriod.ShouldBe(accountingPeriod);
    }
}
