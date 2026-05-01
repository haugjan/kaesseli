using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Budget;

public class GetBudgetEntriesQueryHandlerTests
{
    private static readonly AccountingPeriod ExpectedAccountingPeriod = AccountingPeriod.Create(
        "Test Period",
        default,
        default
    );

    [Fact]
    public async Task Handle_ReturnsCorrectBudgetEntries()
    {
        // Arrange
        var mockRepository = Substitute.For<IBudgetRepository>();
        var accountId = Guid.NewGuid();

        var entriesList = CreateBudgetEntries();

        mockRepository
            .GetBudgetEntries(
                Arg.Is<Guid>(id => id == ExpectedAccountingPeriod.Id),
                Arg.Is<Guid?>(id => id == accountId),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(entriesList);

        var handler = new GetBudgetEntries.Handler(mockRepository);
        var query = new GetBudgetEntries.Query(
            AccountId: accountId,
            AccountType: null,
            AccountingPeriodId: ExpectedAccountingPeriod.Id
        );

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(entriesList.Count);
        result
            .Select(r => r.Id)
            .ToArray()
            .ShouldBeEquivalentTo(entriesList.Select(e => e.Id).ToArray());

        await mockRepository
            .Received(1)
            .GetBudgetEntries(
                Arg.Is<Guid>(id => id == ExpectedAccountingPeriod.Id),
                Arg.Is<Guid?>(id => id == accountId),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            );
    }

    private static List<BudgetEntry> CreateBudgetEntries() =>
        [
            BudgetEntry.Create(
                description: "Description 1",
                amount: 42.42m,
                account: AccountFactory.Create(
                    "Account 1",
                    AccountType.Expense,
                    new AccountIcon("favorite", "blue")
                ),
                accountingPeriod: ExpectedAccountingPeriod
            ),
            BudgetEntry.Create(
                description: "Description 2",
                amount: 24.24m,
                account: AccountFactory.Create(
                    "Account 2",
                    AccountType.Revenue,
                    new AccountIcon("favorite", "blue")
                ),
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            ),
        ];
}
