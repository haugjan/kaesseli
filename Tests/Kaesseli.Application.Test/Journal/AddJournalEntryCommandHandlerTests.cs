using Bogus;
using FluentAssertions;
using Kaesseli.Application.Journal;
using Kaesseli.Domain.Journal;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Journal;

public class AddJournalEntryCommandHandlerTests
{
    private readonly AddJournalEntryCommandHandler _handler;
    private readonly Mock<IJournalRepository> _mockJournalRepository;

    public AddJournalEntryCommandHandlerTests()
    {
        _mockJournalRepository = new Mock<IJournalRepository>();
        _handler = new AddJournalEntryCommandHandler(_mockJournalRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnGuid_WhenJournalEntryIsAddedSuccessfully()
    {
        // Arrange
        var command = new Faker<AddJournalEntryCommand>().UseSeed(seed: 0).Generate();
        var fakeJournalEntry = new Faker<JournalEntry>().UseSeed(seed: 1).Generate();
        var cancellationToken = new CancellationToken();

        _mockJournalRepository.Setup(repo => repo.AddJournalEntry(It.IsAny<JournalEntry>(), cancellationToken))
                              .ReturnsAsync(fakeJournalEntry);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(fakeJournalEntry.Id);
    }

    [Fact]
    public async Task Handle_ShouldCallAddJournalEntryOnRepository()
    {
        // Arrange
        var command = new Faker<AddJournalEntryCommand>().UseSeed(seed: 0).Generate();
        var fakeJournalEntry = new Faker<JournalEntry>().UseSeed(seed: 1).Generate();
        var cancellationToken = new CancellationToken();

        _mockJournalRepository.Setup(repo => repo.AddJournalEntry(It.IsAny<JournalEntry>(), cancellationToken))
                              .ReturnsAsync(fakeJournalEntry);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockJournalRepository.Verify(repo => repo.AddJournalEntry(It.IsAny<JournalEntry>(), cancellationToken), 
                                      times: Times.Once());
        result.Should().Be(fakeJournalEntry.Id);
    }
}