using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Test.Faker;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Budget;

public class SetBudgetHandlerTests
{
    [Fact]
    public async Task Handle_CreatesAndSavesBudgetEntry()
    {
        var account = new SmartFaker<Account>().Generate();
        var period = new SmartFaker<AccountingPeriod>().Generate();
        var query = new SetBudget.Query(500m, "Monatsbudget", account.Id, period.Id);

        var accountRepoMock = new Mock<IAccountRepository>();
        accountRepoMock.Setup(x => x.GetAccount(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        accountRepoMock.Setup(x => x.GetAccountingPeriod(period.Id, It.IsAny<CancellationToken>())).ReturnsAsync(period);

        var budgetRepoMock = new Mock<IBudgetRepository>();
        budgetRepoMock.Setup(x => x.SetBudget(It.IsAny<BudgetEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BudgetEntry b, CancellationToken _) => b);

        var handler = new SetBudget.Handler(budgetRepoMock.Object, accountRepoMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        budgetRepoMock.Verify(x => x.SetBudget(
            It.Is<BudgetEntry>(b => b.Amount == 500m && b.Description == "Monatsbudget"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
