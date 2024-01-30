using FluentAssertions;
using Kässeli.Application.Journal;
using Kässeli.Domain.Entities;
using Kässeli.Domain.Repositories;
using Moq;
using Xunit;

namespace Kässeli.Application.Test.Journal;

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
        var command = new AddJournalEntryCommand
        {

        };
        var fakeJournalEntry = new JournalEntry
        {
            Id = Guid.NewGuid()
        };

        _mockJournalRepository.Setup(repo => repo.AddJournalEntry(It.IsAny<JournalEntry>()))
            .ReturnsAsync(fakeJournalEntry);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(fakeJournalEntry.Id);
    }

    [Fact]
    public async Task Handle_ShouldCallAddJournalEntryOnRepository()
    {
        // Arrange
        var command = new AddJournalEntryCommand
        {
            // Set properties of AddJournalEntryCommand
        };
        var fakeJournalEntry = new JournalEntry
        {
            // Set properties of JournalEntry, including Id
        };

        _mockJournalRepository.Setup(repo => repo.AddJournalEntry(It.IsAny<JournalEntry>()))
            .ReturnsAsync(fakeJournalEntry);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockJournalRepository.Verify(repo => repo.AddJournalEntry(It.IsAny<JournalEntry>()), Times.Once());
    }

    // Weitere Tests können hier folgen
}