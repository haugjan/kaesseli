using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Journal;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class SplitOpenTransactionTests
{
    private readonly IJournalRepository _journalRepoMock = Substitute.For<IJournalRepository>();
    private readonly UpdateOpenTransactionTotal.IHandler _updateOpenTotalMock =
        Substitute.For<UpdateOpenTransactionTotal.IHandler>();
    private readonly SplitOpenTransaction.Handler _handler;

    public SplitOpenTransactionTests() =>
        _handler = new SplitOpenTransaction.Handler(_journalRepoMock, _updateOpenTotalMock);

    [Fact]
    public async Task Handle_SplitsTransactionAndUpdatesOpenTotal()
    {
        var entries = new[]
        {
            new SplitOpenTransactionEntry(Guid.NewGuid(), 50m),
            new SplitOpenTransactionEntry(Guid.NewGuid(), 30m),
        };
        var query = new SplitOpenTransaction.Query(Guid.NewGuid(), Guid.NewGuid(), entries);

        await _handler.Handle(query, CancellationToken.None);

        await _journalRepoMock
            .Received(1)
            .AssignOpenTransaction(
                query.AccountingPeriodId,
                query.TransactionId,
                Arg.Is<IEnumerable<(Guid, decimal)>>(e => e.Count() == 2),
                Arg.Any<CancellationToken>()
            );
        await _updateOpenTotalMock
            .Received(1)
            .Handle(
                Arg.Is<UpdateOpenTransactionTotal.Query>(q => q.Delta == -1),
                Arg.Any<CancellationToken>()
            );
    }
}
