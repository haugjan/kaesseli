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
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly ProcessCamtFileCommandHandler _handler;

    public ProcessCamtFileCommandHandlerTests() =>
        _handler = new ProcessCamtFileCommandHandler(_camtProcessorMock.Object, _journalRepoMock.Object, _accountRepoMock.Object);

    [Fact]
    public async Task Handle_ShouldProcessCamtFileAndReturnEntryIds()
    {
        // Arrange
        var fakeCommand = new SmartFaker<ProcessCamtFileCommand>().Generate();
        var fakeCamtDocument = new SmartFaker<CamtDocument>()
                               .RuleFor(cd => cd.CamtEntries, value: new SmartFaker<CamtEntry>().Generate(count: 2))
                               .Generate();
        var cancellationToken = new CancellationToken();
        _accountRepoMock.Setup(repo => repo.GetAccount(fakeCommand.AccountId, cancellationToken))
                        .ReturnsAsync(
                            (Guid accountId, CancellationToken _) =>
                                new Account { Id = accountId, Name = "Account", Type = AccountType.Expense });
        _camtProcessorMock.Setup(x => x.ReadCamtFile(fakeCommand.Content, fakeCommand.AccountId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(fakeCamtDocument);

        _journalRepoMock.Setup(x => x.AddAccountStatement(It.IsAny<AccountStatement>(), cancellationToken))
                        .ReturnsAsync((AccountStatement accountStatement, CancellationToken ct) => accountStatement);

        // Act
        var result = await _handler.Handle(fakeCommand, cancellationToken);

        // Assert
        _camtProcessorMock.Verify(
            x => x.ReadCamtFile(fakeCommand.Content, fakeCommand.AccountId, It.IsAny<CancellationToken>()),
            Times.Once);
        _journalRepoMock.Verify(x => x.AddAccountStatement(It.IsAny<AccountStatement>(), cancellationToken), Times.Once);
    }
}