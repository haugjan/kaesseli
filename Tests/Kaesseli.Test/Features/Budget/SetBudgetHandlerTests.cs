using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Budget;

public class SetBudgetHandlerTests
{
    [Fact]
    public async Task Handle_CreatesAndSavesBudgetEntry()
    {
        var account = new SmartFaker<Account>().RuleFor(a => a.Type, _ => AccountType.Expense).Generate();
        var period = new SmartFaker<AccountingPeriod>().Generate();
        var query = new SetBudget.Query(500m, "Monatsbudget", account.Id, period.Id);

        var accountRepoMock = Substitute.For<IAccountRepository>();
        accountRepoMock.GetAccount(account.Id, Arg.Any<CancellationToken>()).Returns(account);
        accountRepoMock.GetAccountingPeriod(period.Id, Arg.Any<CancellationToken>()).Returns(period);

        var budgetRepoMock = Substitute.For<IBudgetRepository>();
        budgetRepoMock.SetBudget(Arg.Any<BudgetEntry>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<BudgetEntry>(0));

        var handler = new SetBudget.Handler(budgetRepoMock, accountRepoMock);
        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        await budgetRepoMock.Received(1).SetBudget(
            Arg.Is<BudgetEntry>(b => b.Amount == 500m && b.Description == "Monatsbudget"),
            Arg.Any<CancellationToken>());
    }
}
