using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class GetBudgetEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectBudgetEntries()
    {
        // Arrange
        var mockRepository = new Mock<IBudgetRepository>();
        var accountId = Guid.NewGuid();
        var fromDate = new DateOnly(year: 2020, month: 01, day: 01);
        var toDate = fromDate.AddDays(value: 30);

        var entriesList = CreateBudgetEntries();

        mockRepository.Setup(
                          repo => repo.GetBudgetEntries(
                              It.Is<GetBudgetEntriesRequest>(
                                  r => r.AccountId == accountId && r.FromDate == fromDate && r.ToDate == toDate),
                              It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entriesList);

        var handler = new GetBudgetEntriesQueryHandler(mockRepository.Object);
        var query = new GetBudgetEntriesQuery { AccountId = accountId, FromDate = fromDate, ToDate = toDate };

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
                It.Is<GetBudgetEntriesRequest>(
                    r => r.AccountId == accountId
                      && r.FromDate == fromDate
                      && r.ToDate == toDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static List<BudgetEntry> CreateBudgetEntries() =>
    [
        new()
        {
            Id = Guid.NewGuid(),
            ValueDate = new DateOnly(year: 2000, month: 12, day: 13),
            Description = "Description 1",
            Amount = 42.42m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 1",
                Type = AccountType.Expense,
                Icon = "favorite",
                IconColor = "blue"
            }
        },

        new()
        {
            Id = Guid.NewGuid(),
            ValueDate = new DateOnly(year: 1982, month: 11, day: 3),
            Description = "Description 2",
            Amount = 24.24m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 2",
                Type = AccountType.Revenue,
                Icon = "favorite",
                IconColor = "blue"
            }
        }
    ];
}