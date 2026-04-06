using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Test.Faker;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class ProcessCamtFileDuplicateTests
{
    private readonly Mock<ICamtProcessor> _camtProcessorMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<OpenTransactionAmountChanged.IHandler> _eventHandlerMock = new();
    private readonly ProcessCamtFile.Handler _handler;

    public ProcessCamtFileDuplicateTests() =>
        _handler = new ProcessCamtFile.Handler(
            _camtProcessorMock.Object,
            _transactionRepoMock.Object,
            _accountRepoMock.Object,
            _eventHandlerMock.Object);

    [Fact]
    public async Task Handle_AllDuplicates_ReturnsEmptyGuidAndSkipsSave()
    {
        var command = new SmartFaker<ProcessCamtFile.Query>().Generate();
        var entries = new SmartFaker<FinancialDocumentEntry>().Generate(count: 3);
        var document = new SmartFaker<FinancialDocument>()
            .RuleFor(d => d.Entries, _ => entries)
            .Generate();

        _camtProcessorMock.Setup(x => x.ReadCamtFile(command.Content, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _accountRepoMock.Setup(x => x.GetAccount(command.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Account.Create("Test", AccountType.Asset, new AccountIcon("icon", "blue")));
        _transactionRepoMock.Setup(x => x.GetExistingTransactionReferences(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries.Select(e => e.Reference).ToHashSet());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBe(Guid.Empty);
        _transactionRepoMock.Verify(x => x.AddTransactionSummary(It.IsAny<TransactionSummary>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventHandlerMock.Verify(x => x.Handle(It.IsAny<OpenTransactionAmountChanged.Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SomeDuplicates_ImportsOnlyNewTransactions()
    {
        var command = new SmartFaker<ProcessCamtFile.Query>().Generate();
        var entries = new SmartFaker<FinancialDocumentEntry>().Generate(count: 3);
        var document = new SmartFaker<FinancialDocument>()
            .RuleFor(d => d.Entries, _ => entries)
            .Generate();

        _camtProcessorMock.Setup(x => x.ReadCamtFile(command.Content, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _accountRepoMock.Setup(x => x.GetAccount(command.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Account.Create("Test", AccountType.Asset, new AccountIcon("icon", "blue")));
        _transactionRepoMock.Setup(x => x.GetExistingTransactionReferences(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { entries[0].Reference });
        _transactionRepoMock.Setup(x => x.AddTransactionSummary(It.IsAny<TransactionSummary>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionSummary ts, CancellationToken _) => ts);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        _transactionRepoMock.Verify(x => x.AddTransactionSummary(
            It.Is<TransactionSummary>(ts => ts.Transactions.Count() == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventHandlerMock.Verify(x => x.Handle(
            It.Is<OpenTransactionAmountChanged.Event>(e => e.Amount == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
