using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Test.Faker;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class SetIgnoreTransactionTests
{
    private readonly ITransactionRepository _transactionRepoMock = Substitute.For<ITransactionRepository>();
    private readonly UpdateOpenTransactionTotal.IHandler _updateOpenTotalMock =
        Substitute.For<UpdateOpenTransactionTotal.IHandler>();
    private readonly SetIgnoreTransaction.Handler _handler;

    public SetIgnoreTransactionTests() =>
        _handler = new SetIgnoreTransaction.Handler(_transactionRepoMock, _updateOpenTotalMock);

    [Fact]
    public async Task Handle_IgnoresOpenTransaction_DecrementsOpenTotal()
    {
        var transaction = new SmartFaker<Transaction>()
            .RuleFor(t => t.IsIgnored, value: false)
            .Generate();
        _transactionRepoMock
            .GetTransaction(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);
        _transactionRepoMock
            .HasJournalEntries(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        await _handler.Handle(new SetIgnoreTransaction.Query(transaction.Id, IsIgnored: true), CancellationToken.None);

        await _transactionRepoMock.Received(1)
            .SetTransactionIgnored(transaction.Id, isIgnored: true, Arg.Any<CancellationToken>());
        await _updateOpenTotalMock.Received(1)
            .Handle(Arg.Is<UpdateOpenTransactionTotal.Query>(q => q.Delta == -1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnignoresOpenTransaction_IncrementsOpenTotal()
    {
        var transaction = new SmartFaker<Transaction>()
            .RuleFor(t => t.IsIgnored, value: true)
            .Generate();
        _transactionRepoMock
            .GetTransaction(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);
        _transactionRepoMock
            .HasJournalEntries(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        await _handler.Handle(new SetIgnoreTransaction.Query(transaction.Id, IsIgnored: false), CancellationToken.None);

        await _transactionRepoMock.Received(1)
            .SetTransactionIgnored(transaction.Id, isIgnored: false, Arg.Any<CancellationToken>());
        await _updateOpenTotalMock.Received(1)
            .Handle(Arg.Is<UpdateOpenTransactionTotal.Query>(q => q.Delta == +1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyAssignedTransaction_DoesNotChangeOpenTotal()
    {
        var transaction = new SmartFaker<Transaction>()
            .RuleFor(t => t.IsIgnored, value: false)
            .Generate();
        _transactionRepoMock
            .GetTransaction(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);
        _transactionRepoMock
            .HasJournalEntries(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(new SetIgnoreTransaction.Query(transaction.Id, IsIgnored: true), CancellationToken.None);

        await _transactionRepoMock.Received(1)
            .SetTransactionIgnored(transaction.Id, isIgnored: true, Arg.Any<CancellationToken>());
        await _updateOpenTotalMock.DidNotReceive()
            .Handle(Arg.Any<UpdateOpenTransactionTotal.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_StateUnchanged_IsNoOp()
    {
        var transaction = new SmartFaker<Transaction>()
            .RuleFor(t => t.IsIgnored, value: true)
            .Generate();
        _transactionRepoMock
            .GetTransaction(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);

        await _handler.Handle(new SetIgnoreTransaction.Query(transaction.Id, IsIgnored: true), CancellationToken.None);

        await _transactionRepoMock.DidNotReceive()
            .SetTransactionIgnored(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _updateOpenTotalMock.DidNotReceive()
            .Handle(Arg.Any<UpdateOpenTransactionTotal.Query>(), Arg.Any<CancellationToken>());
    }
}
