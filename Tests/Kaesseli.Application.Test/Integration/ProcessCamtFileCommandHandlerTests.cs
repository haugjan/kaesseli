using FluentAssertions;
using Kaesseli.Application.Integration;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Integration;

public class ProcessCamtFileCommandHandlerTests
{
    private readonly Mock<ICamtProcessor> _camtProcessorMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly ProcessCamtFileCommandHandler _handler;

    public ProcessCamtFileCommandHandlerTests() =>
        _handler = new ProcessCamtFileCommandHandler(_camtProcessorMock.Object, _transactionRepoMock.Object, _accountRepoMock.Object);

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
        _camtProcessorMock.Setup(x => x.ReadCamtFile(fakeCommand.Content, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(fakeCamtDocument);

        _transactionRepoMock.Setup(x => x.AddTransactionSummary(It.IsAny<TransactionSummary>(), cancellationToken))
                        .ReturnsAsync((TransactionSummary transactionSummary, CancellationToken _) => transactionSummary);

        // Act
        var result = await _handler.Handle(fakeCommand, cancellationToken);

        // Assert
        _camtProcessorMock.Verify(
            x => x.ReadCamtFile(fakeCommand.Content, It.IsAny<CancellationToken>()),
            Times.Once);
        _transactionRepoMock.Verify(x => x.AddTransactionSummary(It.IsAny<TransactionSummary>(), cancellationToken), Times.Once);
        result.Should().NotBe(Guid.Empty);
    }
}