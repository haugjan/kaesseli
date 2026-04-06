using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Journal;
using Kaesseli.Test.Faker;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class AssignOpenTransactionTests
{
    private readonly IJournalRepository _journalRepoMock = Substitute.For<IJournalRepository>();
    private readonly ITransactionRepository _transactionRepoMock = Substitute.For<ITransactionRepository>();
    private readonly OpenTransactionAmountChanged.IHandler _eventHandlerMock = Substitute.For<OpenTransactionAmountChanged.IHandler>();
    private readonly AssignOpenTransaction.Handler _handler;

    public AssignOpenTransactionTests() =>
        _handler = new AssignOpenTransaction.Handler(
            _journalRepoMock, _transactionRepoMock, _eventHandlerMock);

    [Fact]
    public async Task Handle_AssignsTransactionAndFiresEvent()
    {
        var transaction = new SmartFaker<Transaction>().Generate();
        var query = new AssignOpenTransaction.Query(Guid.NewGuid(), transaction.Id, Guid.NewGuid());

        _transactionRepoMock.GetTransaction(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);

        await _handler.Handle(query, CancellationToken.None);

        await _journalRepoMock.Received(1).AssignOpenTransaction(
            query.AccountingPeriodId,
            query.TransactionId,
            Arg.Is<IEnumerable<(Guid, decimal)>>(e =>
                e.Single().Item1 == query.OtherAccountId && e.Single().Item2 == transaction.Amount),
            Arg.Any<CancellationToken>());
        await _eventHandlerMock.Received(1).Handle(
            Arg.Is<OpenTransactionAmountChanged.Event>(e => e.Amount == -1),
            Arg.Any<CancellationToken>());
    }
}
