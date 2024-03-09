using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class AssignOpenTransactionCommandHandler : IRequestHandler<AssignOpenTransactionCommand>
{
    private readonly IJournalRepository _journalRepo;
    private readonly ITransactionRepository _tranRepo;

    public AssignOpenTransactionCommandHandler(IJournalRepository journalRepo, ITransactionRepository tranRepo)
    {
        _journalRepo = journalRepo;
        _tranRepo = tranRepo;
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
        //await  _mediator.Publish(
        //    notification: new TransactionAddedNotification(
        //        domainEvent: new Domain.Integration.TransactionAddedEvent
        //        {
        //            TransactionId = request.TransactionId, 
        //            AccountId = request.OtherAccountId
        //        }),
        //    cancellationToken: cancellationToken);
    }
}