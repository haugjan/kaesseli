using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.Domain.Budget;
using Kaesseli.TestUtilities.Faker;
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

        var faker = new SmartFaker<BudgetEntry>();
        var entriesList = faker.Generate(count: 5);
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
                It.Is<GetBudgetEntriesRequest>(r => r.AccountId == accountId && r.FromDate == fromDate && r.ToDate == toDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}