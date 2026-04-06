using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Journal;
using Moq;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class SplitOpenTransactionTests
{
    private readonly Mock<IJournalRepository> _journalRepoMock = new();
    private readonly Mock<OpenTransactionAmountChanged.IHandler> _eventHandlerMock = new();
    private readonly SplitOpenTransaction.Handler _handler;

    public SplitOpenTransactionTests() =>
        _handler = new SplitOpenTransaction.Handler(_journalRepoMock.Object, _eventHandlerMock.Object);

    [Fact]
    public async Task Handle_SplitsTransactionAndFiresEvent()
    {
        var entries = new[]
        {
            new SplitOpenTransactionEntry(Guid.NewGuid(), 50m),
            new SplitOpenTransactionEntry(Guid.NewGuid(), 30m),
        };
        var query = new SplitOpenTransaction.Query(Guid.NewGuid(), Guid.NewGuid(), entries);

        await _handler.Handle(query, CancellationToken.None);

        _journalRepoMock.Verify(x => x.AssignOpenTransaction(
            query.AccountingPeriodId,
            query.TransactionId,
            It.Is<IEnumerable<(Guid, decimal)>>(e => e.Count() == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventHandlerMock.Verify(x => x.Handle(
            It.Is<OpenTransactionAmountChanged.Event>(e => e.Amount == -1),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
