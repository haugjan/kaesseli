using Kaesseli.Application.Journal;
using Kaesseli.Application.Common;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Journal;

public class AddJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IJournalRepository>();
        var dateTimeService = new Mock<IDateTimeService>();
        var command = new SmartFaker<AddJournalEntryCommand>().Generate();
        var cancellationToken = new CancellationToken();

        mockRepo.Setup(
                    repo => repo.AddJournalEntry(
                        It.Is<JournalEntry>(a => a.Amount == command.Amount && a.Description == command.Description),
                        cancellationToken))
                .ReturnsAsync((JournalEntry newJournalEntry, CancellationToken _) => newJournalEntry);

        var handler = new AddJournalEntryCommandHandler(mockRepo.Object, dateTimeService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(
            repo => repo.AddJournalEntry(
                It.Is<JournalEntry>(
                    entry => entry.Amount == command.Amount
                          && entry.Description == command.Description
                          && entry.ValueDate == command.ValueDate
                             && entry.Id == result),
                cancellationToken));
    }

    [Fact]
    public async Task Handle_EmptyValueDate_ShouldAddAccountWithCurrentDate()
    {
        // Arrange
        var mockRepo = new Mock<IJournalRepository>();
        var dateTimeService = new Mock<IDateTimeService>();
        var command = new SmartFaker<AddJournalEntryCommand>().RuleFor(c => c.ValueDate, _ => null).Generate();
        var cancellationToken = new CancellationToken();
        var currentDay = new DateOnly(year: 1982, month: 11, day: 3);

        mockRepo.Setup(
                    repo => repo.AddJournalEntry(
                        It.Is<JournalEntry>(a => a.Amount == command.Amount && a.Description == command.Description),
                        cancellationToken))
                .ReturnsAsync((JournalEntry newJournalEntry, CancellationToken _) => newJournalEntry);
        dateTimeService.Setup(dts => dts.ToDay).Returns(currentDay);
        var handler = new AddJournalEntryCommandHandler(mockRepo.Object, dateTimeService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(
            repo => repo.AddJournalEntry(
                It.Is<JournalEntry>(
                    entry => entry.Amount == command.Amount
                          && entry.Description == command.Description
                          && entry.ValueDate == currentDay
                             && entry.Id== result),
                cancellationToken));
    }
}