using Kaesseli.Features.Journal;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Journal;

public class GetJournalEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectJournalEntries()
    {
        // Arrange
        var mockRepository = Substitute.For<IJournalRepository>();
        var expectedPeriodId = Guid.NewGuid();

        var faker = new SmartFaker<JournalEntry>();
        var entriesList = faker.Generate(count: 5);
        mockRepository
            .GetJournalEntries(
                Arg.Is<Guid>(id => id == expectedPeriodId),
                Arg.Any<Guid?>(),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(entriesList);

        var handler = new GetJournalEntries.Handler(mockRepository);
        var query = new GetJournalEntries.Query(AccountingPeriodId: expectedPeriodId, AccountId: null, AccountType: null);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(entriesList.Count);
        result.Select(r => r.Id).ToArray().ShouldBeEquivalentTo(entriesList.Select(e => e.Id).ToArray());

        await mockRepository.Received(1)
            .GetJournalEntries(
                Arg.Is<Guid>(id => id == expectedPeriodId),
                Arg.Any<Guid?>(),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            );
    }
}
