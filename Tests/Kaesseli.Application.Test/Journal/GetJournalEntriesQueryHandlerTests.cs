using FluentAssertions;
using Kaesseli.Application.Journal;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Journal;

public class GetJournalEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectJournalEntries()
    {
        // Arrange
        var mockRepository = new Mock<IJournalRepository>();
        var expectedPeriodId = Guid.NewGuid();

        var faker = new SmartFaker<JournalEntry>();
        var entriesList = faker.Generate(count: 5);
        mockRepository.Setup(
                          repo => repo.GetJournalEntries(
                              It.Is<GetJournalEntriesRequest>(
                                  r => r.AccountingPeriodId == expectedPeriodId),
                              It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entriesList);

        var handler = new GetJournalEntriesQueryHandler(mockRepository.Object);
        var query = new GetJournalEntriesQuery
        {

            AccountingPeriodId = expectedPeriodId,
            AccountId = null,
            AccountType = null
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
            repo => repo.GetJournalEntries(
                It.Is<GetJournalEntriesRequest>(
                    r => r.AccountingPeriodId == expectedPeriodId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}