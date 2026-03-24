using FluentAssertions;
using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Integration;

public class ProcessCamtFileCommandHandlerTests
{
    private readonly Mock<ICamtProcessor> _camtProcessorMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<OpenTransactionAmountChanged.IHandler> _eventHandlerMock = new();
    private readonly ProcessCamtFile.Handler _handler;

    public ProcessCamtFileCommandHandlerTests() =>
        _handler = new ProcessCamtFile.Handler(
            _camtProcessorMock.Object,
            _transactionRepoMock.Object,
            _accountRepoMock.Object,
            _eventHandlerMock.Object
        );

    [Fact]
    public async Task Handle_ShouldProcessCamtFileAndReturnEntryIds()
    {
        // Arrange
        var fakeCommand = new SmartFaker<ProcessCamtFile.Query>().Generate();
        var financialDocument = new SmartFaker<FinancialDocument>()
            .RuleFor(
                cd => cd.Entries,
                value: new SmartFaker<FinancialDocumentEntry>().Generate(count: 2)
            )
            .Generate();
        var cancellationToken = new CancellationToken();
        _accountRepoMock
            .Setup(repo => repo.GetAccount(fakeCommand.AccountId, cancellationToken))
            .ReturnsAsync(
                (Guid accountId, CancellationToken _) =>
                    new Account
                    {
                        Id = accountId,
                        Name = "Account",
                        Type = AccountType.Expense,
                        Icon = new AccountIcon("favorite", "blue"),
                    }
            );
        _camtProcessorMock
            .Setup(x => x.ReadCamtFile(fakeCommand.Content, It.IsAny<CancellationToken>()))
            .ReturnsAsync(financialDocument);

        _transactionRepoMock
            .Setup(x => x.AddTransactionSummary(It.IsAny<TransactionSummary>(), cancellationToken))
            .ReturnsAsync(
                (TransactionSummary transactionSummary, CancellationToken _) => transactionSummary
            );

        // Act
        var result = await _handler.Handle(fakeCommand, cancellationToken);

        // Assert
        _camtProcessorMock.Verify(
            x => x.ReadCamtFile(fakeCommand.Content, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _transactionRepoMock.Verify(
            x => x.AddTransactionSummary(It.IsAny<TransactionSummary>(), cancellationToken),
            Times.Once
        );
        result.Should().NotBe(Guid.Empty);
    }
}
