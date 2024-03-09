using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class SplitOpenTransactionCommandHandler : IRequestHandler<SplitOpenTransactionCommand>
{
    private readonly IJournalRepository _journalRepo;

    public SplitOpenTransactionCommandHandler(IJournalRepository journalRepo) =>
        _journalRepo = journalRepo;

    public async Task Handle(SplitOpenTransactionCommand request, CancellationToken cancellationToken)
    {
        var entries = request.Entries.Select(
            entry => new AssignOpenTransactionEntry { OtherAccountId = entry.OtherAccountId, Amount = entry.Amount });
        await _journalRepo.AssignOpenTransaction(request.AccountingPeriodId, request.TransactionId, entries, cancellationToken);
    }
}