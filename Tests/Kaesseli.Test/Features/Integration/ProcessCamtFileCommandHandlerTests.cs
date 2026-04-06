using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class ProcessCamtFileCommandHandlerTests
{
    private readonly ICamtProcessor _camtProcessorMock = Substitute.For<ICamtProcessor>();
    private readonly ITransactionRepository _transactionRepoMock = Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accountRepoMock = Substitute.For<IAccountRepository>();
    private readonly OpenTransactionAmountChanged.IHandler _eventHandlerMock = Substitute.For<OpenTransactionAmountChanged.IHandler>();
    private readonly ProcessCamtFile.Handler _handler;

    public ProcessCamtFileCommandHandlerTests() =>
        _handler = new ProcessCamtFile.Handler(
            _camtProcessorMock,
            _transactionRepoMock,
            _accountRepoMock,
            _eventHandlerMock
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
            .GetAccount(fakeCommand.AccountId, cancellationToken)
            .Returns(
                Account.Create("Account", AccountType.Expense, new AccountIcon("favorite", "blue"))
            );
        _camtProcessorMock
            .ReadCamtFile(fakeCommand.Content, Arg.Any<CancellationToken>())
            .Returns(financialDocument);

        _transactionRepoMock
            .GetExistingTransactionReferences(Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>());
        _transactionRepoMock
            .AddTransactionSummary(Arg.Any<TransactionSummary>(), cancellationToken)
            .Returns(callInfo => callInfo.ArgAt<TransactionSummary>(0));

        // Act
        var result = await _handler.Handle(fakeCommand, cancellationToken);

        // Assert
        await _camtProcessorMock.Received(1)
            .ReadCamtFile(fakeCommand.Content, Arg.Any<CancellationToken>());
        await _transactionRepoMock.Received(1)
            .AddTransactionSummary(Arg.Any<TransactionSummary>(), cancellationToken);
        result.ShouldNotBe(Guid.Empty);
    }
}
