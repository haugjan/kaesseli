using Kaesseli.Application.Integration.TransactionAddedEvent;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class AssignOpenTransactionCommandHandler : IRequestHandler<AssignOpenTransactionCommand>
{
    private readonly IJournalRepository _journalRepo;
    private readonly IMediator _mediator;

    public AssignOpenTransactionCommandHandler(IJournalRepository journalRepo, IMediator mediator)
    {
        _journalRepo = journalRepo;
        _mediator = mediator;
    }

    public async Task Handle(AssignOpenTransactionCommand request, CancellationToken cancellationToken)
    {
        await _journalRepo.AssignOpenTransaction(request.TransactionId, request.OtherAccountId, request.AccountingPeriodId, cancellationToken);
        await  _mediator.Publish(
            notification: new TransactionAddedNotification(
                domainEvent: new Domain.Integration.TransactionAddedEvent
                {
                    TransactionId = request.TransactionId, 
                    AccountId = request.OtherAccountId
                }),
            cancellationToken: cancellationToken);
    }
}