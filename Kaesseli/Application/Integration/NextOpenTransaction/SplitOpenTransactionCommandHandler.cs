using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public interface ISplitOpenTransactionCommandHandler
{
    Task Handle(SplitOpenTransactionCommand request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class SplitOpenTransactionCommandHandler : ISplitOpenTransactionCommandHandler
{
    private readonly IJournalRepository _journalRepo;
    private readonly IOpenTransactionAmountChangedEventHandler _eventHandler;

    public SplitOpenTransactionCommandHandler(IJournalRepository journalRepo, IOpenTransactionAmountChangedEventHandler eventHandler)
    {
        _journalRepo = journalRepo;
        _eventHandler = eventHandler;
    }

    public async Task Handle(SplitOpenTransactionCommand request, CancellationToken cancellationToken)
    {
        var entries = request.Entries.Select(
            entry => (entry.OtherAccountId, entry.Amount));
        await _journalRepo.AssignOpenTransaction(request.AccountingPeriodId, request.TransactionId, entries, cancellationToken);
        await _eventHandler.Handle(
            notification: new OpenTransactionAmountChangedEvent { Amount = -1 },
            cancellationToken);
    }
}
