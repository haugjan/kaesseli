using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Journal;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class AssignOpenTransactionTests
{
    private readonly Mock<IJournalRepository> _journalRepoMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepoMock = new();
    private readonly Mock<OpenTransactionAmountChanged.IHandler> _eventHandlerMock = new();
    private readonly AssignOpenTransaction.Handler _handler;

    public AssignOpenTransactionTests() =>
        _handler = new AssignOpenTransaction.Handler(
            _journalRepoMock.Object, _transactionRepoMock.Object, _eventHandlerMock.Object);

    [Fact]
    public async Task Handle_AssignsTransactionAndFiresEvent()
    {
        var transaction = new SmartFaker<Transaction>().Generate();
        var query = new AssignOpenTransaction.Query(Guid.NewGuid(), transaction.Id, Guid.NewGuid());

        _transactionRepoMock.Setup(x => x.GetTransaction(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        await _handler.Handle(query, CancellationToken.None);

        _journalRepoMock.Verify(x => x.AssignOpenTransaction(
            query.AccountingPeriodId,
            query.TransactionId,
            It.Is<IEnumerable<(Guid, decimal)>>(e =>
                e.Single().Item1 == query.OtherAccountId && e.Single().Item2 == transaction.Amount),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventHandlerMock.Verify(x => x.Handle(
            It.Is<OpenTransactionAmountChanged.Event>(e => e.Amount == -1),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
