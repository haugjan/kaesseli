using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public interface IAssignOpenTransactionCommandHandler
{
    Task Handle(AssignOpenTransactionCommand request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class AssignOpenTransactionCommandHandler : IAssignOpenTransactionCommandHandler
{
    private readonly IJournalRepository _journalRepo;
    private readonly ITransactionRepository _tranRepo;
    private readonly IOpenTransactionAmountChangedEventHandler _eventHandler;

    public AssignOpenTransactionCommandHandler(IJournalRepository journalRepo, ITransactionRepository tranRepo, IOpenTransactionAmountChangedEventHandler eventHandler)
    {
        _journalRepo = journalRepo;
        _tranRepo = tranRepo;
        _eventHandler = eventHandler;
    }

    public async Task Handle(AssignOpenTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _tranRepo.GetTransaction(request.TransactionId, cancellationToken);
        var entries = new List<(Guid OtherAccountId, decimal Amount)>
        {
            (request.OtherAccountId, transaction.Amount)
        };
        await _journalRepo.AssignOpenTransaction(
            request.AccountingPeriodId,
            request.TransactionId,
            entries,
            cancellationToken);
        await _eventHandler.Handle(
            notification: new OpenTransactionAmountChangedEvent { Amount = -1 },
            cancellationToken);
    }
}
