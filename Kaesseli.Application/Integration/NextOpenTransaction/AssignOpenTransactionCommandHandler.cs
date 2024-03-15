using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class AssignOpenTransactionCommandHandler : IRequestHandler<AssignOpenTransactionCommand>
{
    private readonly IJournalRepository _journalRepo;
    private readonly ITransactionRepository _tranRepo;
    private readonly IMediator _mediator;

    public AssignOpenTransactionCommandHandler(IJournalRepository journalRepo, ITransactionRepository tranRepo, IMediator mediator)
    {
        _journalRepo = journalRepo;
        _tranRepo = tranRepo;
        _mediator = mediator;
    }

    public async Task Handle(AssignOpenTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _tranRepo.GetTransaction(request.TransactionId, cancellationToken);
        var entries = new List<AssignOpenTransactionEntry>
        {
            new() { OtherAccountId = request.OtherAccountId, Amount = transaction.Amount }
        };
        await _journalRepo.AssignOpenTransaction(
            request.AccountingPeriodId,
            request.TransactionId,
            entries,
            cancellationToken);
        await _mediator.Publish(
            notification: new OpenTransactionAmountChangedEvent { Amount = -1 },
            cancellationToken);
    }
}