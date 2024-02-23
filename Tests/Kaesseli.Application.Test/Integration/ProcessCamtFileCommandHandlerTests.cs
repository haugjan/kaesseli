using FluentAssertions;
using Kaesseli.Application.Integration;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Integration;

public class ProcessCamtFileCommandHandlerTests
{
    private readonly Mock<ICamtProcessor> _camtProcessorMock = new();
    private readonly Mock<IJournalRepository> _journalRepoMock = new();
    private readonly Mock<IAccountRepository> _accountRepo = new();
    private readonly ProcessCamtFileCommandHandler _handler;

    public ProcessCamtFileCommandHandlerTests()
    {
        _handler = new ProcessCamtFileCommandHandler(_camtProcessorMock.Object, _journalRepoMock.Object, _accountRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldProcessCamtFileAndReturnEntryIds()
    {
        // Arrange
        var fakeCommand = new SmartFaker<ProcessCamtFileCommand>().Generate();
        var fakePreJournalEntries = new SmartFaker<CamtEntry>().Generate(count: 2);
        var cancellationToken = new CancellationToken();
        _camtProcessorMock.Setup(x => x.ReadCamtFile(fakeCommand.Content, fakeCommand.AccountId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(fakePreJournalEntries);

        _journalRepoMock.Setup(x => x.AddPreJournalEntry(It.IsAny<PreJournalEntry>(), cancellationToken))
                        .ReturnsAsync((PreJournalEntry preJournalEntry, CancellationToken ct) => preJournalEntry);

        // Act
        var result = (await _handler.Handle(fakeCommand, cancellationToken)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(fakePreJournalEntries.Count);

        _camtProcessorMock.Verify(x => x.ReadCamtFile(fakeCommand.Content, fakeCommand.AccountId, It.IsAny<CancellationToken>()), Times.Once);
        _journalRepoMock.Verify(x => x.AddPreJournalEntry(It.IsAny<PreJournalEntry>(), cancellationToken), times: Times.Exactly(fakePreJournalEntries.Count));
    }
}