using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class SplitOpenTransactionCommandHandler : IRequestHandler<SplitOpenTransactionCommand>
{
    private readonly IJournalRepository _journalRepo;
    private readonly IMediator _mediator;

    public SplitOpenTransactionCommandHandler(IJournalRepository journalRepo, IMediator mediator)
    {
        _journalRepo = journalRepo;
        _mediator = mediator;
    }

    public async Task Handle(SplitOpenTransactionCommand request, CancellationToken cancellationToken)
    {
        var entries = request.Entries.Select(
            entry => (entry.OtherAccountId, entry.Amount));
        await _journalRepo.AssignOpenTransaction(request.AccountingPeriodId, request.TransactionId, entries, cancellationToken);
        await _mediator.Publish(
            notification: new OpenTransactionAmountChangedEvent { Amount = -1 },
            cancellationToken);
    }
}