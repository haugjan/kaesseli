using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class GetBudgetEntriesQueryHandlerTests
{
    private static readonly Guid ExpectedAccountingPeriodId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ReturnsCorrectBudgetEntries()
    {
        // Arrange
        var mockRepository = new Mock<IBudgetRepository>();
        var accountId = Guid.NewGuid();

        var entriesList = CreateBudgetEntries();

        mockRepository.Setup(
                          repo => repo.GetBudgetEntries(
                              It.Is<Guid>(id => id == ExpectedAccountingPeriodId),
                              It.Is<Guid?>(id => id == accountId), It.IsAny<AccountType?>(),
                              It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entriesList);

        var handler = new GetBudgetEntriesQueryHandler(mockRepository.Object);
        var query = new GetBudgetEntriesQuery
        {
            AccountId = accountId,
            AccountType = null,
            AccountingPeriodId = ExpectedAccountingPeriodId
        };

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(entriesList.Count);
        result.Should()
              .BeEquivalentTo(
                  entriesList,
                  options => options.Using<DateTime>(
                                        ctx => ctx.Subject.Should()
                                                  .BeCloseTo(ctx.Expectation, precision: TimeSpan.FromSeconds(value: 1)))
                                    .WhenTypeIs<DateTime>()
                                    .ExcludingMissingMembers());

        mockRepository.Verify(
            repo => repo.GetBudgetEntries(
                It.Is<Guid>(id => id == ExpectedAccountingPeriodId),
                It.Is<Guid?>(id => id == accountId), It.IsAny<AccountType?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static List<BudgetEntry> CreateBudgetEntries() =>
    [
        new()
        {
            Id = Guid.NewGuid(),
            Description = "Description 1",
            Amount = 42.42m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 1",
                Type = AccountType.Expense,
                Icon = new AccountIcon("favorite", "blue")
            },
            AccountingPeriod = new AccountingPeriod
            {
                Id = ExpectedAccountingPeriodId,
                FromInclusive = default,
                ToInclusive = default,
                Description = string.Empty
            }
        },

        new()
        {
            Id = Guid.NewGuid(),
            Description = "Description 2",
            Amount = 24.24m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 2",
                Type = AccountType.Revenue,
                Icon = new AccountIcon("favorite", "blue")
            },
            AccountingPeriod = new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FromInclusive = default,
                ToInclusive = default,
                Description = string.Empty
            }
        }
    ];
}
