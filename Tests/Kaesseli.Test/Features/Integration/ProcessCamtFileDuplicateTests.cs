using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class ProcessCamtFileDuplicateTests
{
    private readonly ICamtProcessor _camtProcessorMock = Substitute.For<ICamtProcessor>();
    private readonly ITransactionRepository _transactionRepoMock =
        Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accountRepoMock = Substitute.For<IAccountRepository>();
    private readonly UpdateOpenTransactionTotal.IHandler _updateOpenTotalMock =
        Substitute.For<UpdateOpenTransactionTotal.IHandler>();
    private readonly ProcessCamtFile.Handler _handler;

    public ProcessCamtFileDuplicateTests() =>
        _handler = new ProcessCamtFile.Handler(
            _camtProcessorMock,
            _transactionRepoMock,
            _accountRepoMock,
            _updateOpenTotalMock
        );

    [Fact]
    public async Task Handle_AllDuplicates_ReturnsEmptyGuidAndSkipsSave()
    {
        var command = new SmartFaker<ProcessCamtFile.Query>().Generate();
        var entries = new SmartFaker<FinancialDocumentEntry>().Generate(count: 3);
        var document = new SmartFaker<FinancialDocument>()
            .RuleFor(d => d.Entries, _ => entries)
            .Generate();

        _camtProcessorMock
            .ReadCamtFile(command.Content, Arg.Any<CancellationToken>())
            .Returns(document);
        _accountRepoMock
            .GetAccount(command.AccountId, Arg.Any<CancellationToken>())
            .Returns(
                AccountFactory.Create("Test", AccountType.Asset, new AccountIcon("icon", "blue"))
            );
        _transactionRepoMock
            .GetExistingTransactionReferences(Arg.Any<CancellationToken>())
            .Returns(entries.Select(e => e.Reference).ToHashSet());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBe(Guid.Empty);
        await _transactionRepoMock
            .DidNotReceive()
            .AddTransactionSummary(Arg.Any<TransactionSummary>(), Arg.Any<CancellationToken>());
        await _updateOpenTotalMock
            .DidNotReceive()
            .Handle(Arg.Any<UpdateOpenTransactionTotal.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SomeDuplicates_ImportsOnlyNewTransactions()
    {
        var command = new SmartFaker<ProcessCamtFile.Query>().Generate();
        var entries = new SmartFaker<FinancialDocumentEntry>().Generate(count: 3);
        var document = new SmartFaker<FinancialDocument>()
            .RuleFor(d => d.Entries, _ => entries)
            .Generate();

        _camtProcessorMock
            .ReadCamtFile(command.Content, Arg.Any<CancellationToken>())
            .Returns(document);
        _accountRepoMock
            .GetAccount(command.AccountId, Arg.Any<CancellationToken>())
            .Returns(
                AccountFactory.Create("Test", AccountType.Asset, new AccountIcon("icon", "blue"))
            );
        _transactionRepoMock
            .GetExistingTransactionReferences(Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { entries[0].Reference });
        _transactionRepoMock
            .AddTransactionSummary(Arg.Any<TransactionSummary>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<TransactionSummary>(0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        await _transactionRepoMock
            .Received(1)
            .AddTransactionSummary(
                Arg.Is<TransactionSummary>(ts => ts.Transactions.Count() == 2),
                Arg.Any<CancellationToken>()
            );
        await _updateOpenTotalMock
            .Received(1)
            .Handle(
                Arg.Is<UpdateOpenTransactionTotal.Query>(q => q.Delta == 2),
                Arg.Any<CancellationToken>()
            );
    }
}
