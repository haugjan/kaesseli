using Kaesseli.Features.Journal;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Journal;

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
        mockRepository
            .Setup(repo =>
                repo.GetJournalEntries(
                    It.Is<Guid>(id => id == expectedPeriodId),
                    It.IsAny<Guid?>(),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(entriesList);

        var handler = new GetJournalEntries.Handler(mockRepository.Object);
        var query = new GetJournalEntries.Query(AccountingPeriodId: expectedPeriodId, AccountId: null, AccountType: null);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(entriesList.Count);
        result.Select(r => r.Id).ToArray().ShouldBeEquivalentTo(entriesList.Select(e => e.Id).ToArray());

        mockRepository.Verify(
            repo =>
                repo.GetJournalEntries(
                    It.Is<Guid>(id => id == expectedPeriodId),
                    It.IsAny<Guid?>(),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
