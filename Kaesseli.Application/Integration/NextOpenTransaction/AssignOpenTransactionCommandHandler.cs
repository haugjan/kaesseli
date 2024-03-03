using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class AssignOpenTransactionCommandHandler : IRequestHandler<AssignOpenTransactionCommand>
{
    private readonly IJournalRepository _journalRepo;

    public AssignOpenTransactionCommandHandler(IJournalRepository journalRepo) =>
        _journalRepo = journalRepo;

    public async Task Handle(AssignOpenTransactionCommand request, CancellationToken cancellationToken)
    {
        await _journalRepo.AssignOpenTransaction(request.TransactionId, request.OtherAccountId, cancellationToken);
    }
}