using Kaesseli.Features.Journal;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Journal;

public class AddJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockJournalRepo = new Mock<IJournalRepository>();
        var mockAccountRepo = new Mock<IAccountRepository>();
        var command = new SmartFaker<AddJournalEntry.Query>().Generate();
        var cancellationToken = new CancellationToken();

        mockJournalRepo
            .Setup(repo =>
                repo.AddJournalEntry(
                    It.Is<JournalEntry>(a =>
                        a.Amount == command.Amount && a.Description == command.Description
                    ),
                    cancellationToken
                )
            )
            .ReturnsAsync((JournalEntry newJournalEntry, CancellationToken _) => newJournalEntry);

        var handler = new AddJournalEntry.Handler(
            mockJournalRepo.Object,
            mockAccountRepo.Object,
            TimeProvider.System
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockJournalRepo.Verify(repo =>
            repo.AddJournalEntry(
                It.Is<JournalEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.ValueDate == command.ValueDate
                    && entry.Id == result
                ),
                cancellationToken
            )
        );
    }

    [Fact]
    public async Task Handle_EmptyValueDate_ShouldAddAccountWithCurrentDate()
    {
        // Arrange
        var mockJournalRepo = new Mock<IJournalRepository>();
        var mockAccountRepo = new Mock<IAccountRepository>();
        var currentDay = new DateOnly(year: 1982, month: 11, day: 3);
        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(currentDay.ToDateTime(TimeOnly.MinValue)));
        var command = new SmartFaker<AddJournalEntry.Query>()
            .RuleFor(c => c.ValueDate, _ => null)
            .Generate();
        var cancellationToken = new CancellationToken();

        mockJournalRepo
            .Setup(repo =>
                repo.AddJournalEntry(
                    It.Is<JournalEntry>(a =>
                        a.Amount == command.Amount && a.Description == command.Description
                    ),
                    cancellationToken
                )
            )
            .ReturnsAsync((JournalEntry newJournalEntry, CancellationToken _) => newJournalEntry);
        var handler = new AddJournalEntry.Handler(
            mockJournalRepo.Object,
            mockAccountRepo.Object,
            fakeTimeProvider
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockJournalRepo.Verify(repo =>
            repo.AddJournalEntry(
                It.Is<JournalEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.ValueDate == currentDay
                    && entry.Id == result
                ),
                cancellationToken
            )
        );
    }
}
