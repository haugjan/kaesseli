using Kaesseli.Features.Budget;
using Kaesseli.Features.Accounts;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Budget;

public class GetBudgetEntriesQueryHandlerTests
{
    private static readonly AccountingPeriod ExpectedAccountingPeriod =
        AccountingPeriod.Create("Test Period", default, default);

    [Fact]
    public async Task Handle_ReturnsCorrectBudgetEntries()
    {
        // Arrange
        var mockRepository = new Mock<IBudgetRepository>();
        var accountId = Guid.NewGuid();

        var entriesList = CreateBudgetEntries();

        mockRepository
            .Setup(repo =>
                repo.GetBudgetEntries(
                    It.Is<Guid>(id => id == ExpectedAccountingPeriod.Id),
                    It.Is<Guid?>(id => id == accountId),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(entriesList);

        var handler = new GetBudgetEntries.Handler(mockRepository.Object);
        var query = new GetBudgetEntries.Query(AccountId: accountId, AccountType: null, AccountingPeriodId: ExpectedAccountingPeriod.Id);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(entriesList.Count);
        result.Select(r => r.Id).ToArray().ShouldBeEquivalentTo(entriesList.Select(e => e.Id).ToArray());

        mockRepository.Verify(
            repo =>
                repo.GetBudgetEntries(
                    It.Is<Guid>(id => id == ExpectedAccountingPeriod.Id),
                    It.Is<Guid?>(id => id == accountId),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static List<BudgetEntry> CreateBudgetEntries() =>
        [
            BudgetEntry.Create(
                description: "Description 1",
                amount: 42.42m,
                account: Account.Create("Account 1", AccountType.Expense, new AccountIcon("favorite", "blue")),
                accountingPeriod: ExpectedAccountingPeriod
            ),
            BudgetEntry.Create(
                description: "Description 2",
                amount: 24.24m,
                account: Account.Create("Account 2", AccountType.Revenue, new AccountIcon("favorite", "blue")),
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            ),
        ];
}
