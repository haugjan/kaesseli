using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class ProcessCamtFileCommandHandlerTests
{
    private readonly ICamtProcessor _camtProcessorMock = Substitute.For<ICamtProcessor>();
    private readonly ITransactionRepository _transactionRepoMock =
        Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accountRepoMock = Substitute.For<IAccountRepository>();
    private readonly UpdateOpenTransactionTotal.IHandler _updateOpenTotalMock =
        Substitute.For<UpdateOpenTransactionTotal.IHandler>();
    private readonly ProcessCamtFile.Handler _handler;

    public ProcessCamtFileCommandHandlerTests() =>
        _handler = new ProcessCamtFile.Handler(
            _camtProcessorMock,
            _transactionRepoMock,
            _accountRepoMock,
            _updateOpenTotalMock
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
            .RuleFor(cd => cd.HasBalanceInfo, value: false)
            .Generate();
        var cancellationToken = new CancellationToken();
        _accountRepoMock
            .GetAccount(fakeCommand.AccountId, cancellationToken)
            .Returns(
                AccountFactory.Create(
                    "Account",
                    AccountType.Expense,
                    new AccountIcon("favorite", "blue")
                )
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
        await _camtProcessorMock
            .Received(1)
            .ReadCamtFile(fakeCommand.Content, Arg.Any<CancellationToken>());
        await _transactionRepoMock
            .Received(1)
            .AddTransactionSummary(Arg.Any<TransactionSummary>(), cancellationToken);
        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_BalanceMismatch_ThrowsAndDoesNotPersist()
    {
        var query = new ProcessCamtFile.Query(Stream.Null, Guid.NewGuid(), IgnoreBalanceMismatch: false);
        var inconsistent = new FinancialDocument
        {
            Entries =
            [
                new FinancialDocumentEntry
                {
                    Description = "x", RawText = "x", Amount = 10m,
                    ValueDate = new DateOnly(2026, 1, 1), BookDate = new DateOnly(2026, 1, 1),
                    Reference = "r", TransactionCode = "PMNT", TransactionCodeDetail = "",
                    Debtor = null, Creditor = null
                }
            ],
            BalanceBefore = 100m,
            BalanceAfter = 200m,
            ValueDateFrom = new DateOnly(2026, 1, 1),
            ValueDateTo = new DateOnly(2026, 1, 1),
            Reference = "ref",
            HasBalanceInfo = true
        };
        _camtProcessorMock.ReadCamtFile(query.Content, Arg.Any<CancellationToken>())
                          .Returns(inconsistent);

        await Should.ThrowAsync<BalanceMismatchException>(
            () => _handler.Handle(query, CancellationToken.None));
        await _transactionRepoMock.DidNotReceiveWithAnyArgs()
                                  .AddTransactionSummary(default!, default);
    }

    [Fact]
    public async Task Handle_BalanceMismatch_WhenIgnored_PersistsAnyway()
    {
        var query = new ProcessCamtFile.Query(Stream.Null, Guid.NewGuid(), IgnoreBalanceMismatch: true);
        var inconsistent = new FinancialDocument
        {
            Entries =
            [
                new FinancialDocumentEntry
                {
                    Description = "x", RawText = "x", Amount = 10m,
                    ValueDate = new DateOnly(2026, 1, 1), BookDate = new DateOnly(2026, 1, 1),
                    Reference = "r", TransactionCode = "PMNT", TransactionCodeDetail = "",
                    Debtor = null, Creditor = null
                }
            ],
            BalanceBefore = 100m,
            BalanceAfter = 200m,
            ValueDateFrom = new DateOnly(2026, 1, 1),
            ValueDateTo = new DateOnly(2026, 1, 1),
            Reference = "ref",
            HasBalanceInfo = true
        };
        _camtProcessorMock.ReadCamtFile(query.Content, Arg.Any<CancellationToken>())
                          .Returns(inconsistent);
        _accountRepoMock.GetAccount(query.AccountId, Arg.Any<CancellationToken>())
                        .Returns(AccountFactory.Create("A", AccountType.Asset, new AccountIcon("favorite", "blue")));
        _transactionRepoMock.GetExistingTransactionReferences(Arg.Any<CancellationToken>())
                            .Returns(new HashSet<string>());
        _transactionRepoMock.AddTransactionSummary(Arg.Any<TransactionSummary>(), Arg.Any<CancellationToken>())
                            .Returns(c => c.ArgAt<TransactionSummary>(0));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        await _transactionRepoMock.Received(1)
                                  .AddTransactionSummary(Arg.Any<TransactionSummary>(), Arg.Any<CancellationToken>());
    }
}
