using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Integration.NextOpenTransaction;

public static class SplitOpenTransaction
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(Guid AccountingPeriodId, Guid TransactionId, IEnumerable<SplitOpenTransactionEntry> Entries);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IJournalRepository journalRepo, OpenTransactionAmountChanged.IHandler eventHandler) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var entries = request.Entries.Select(entry => (entry.OtherAccountId, entry.Amount));
            await journalRepo.AssignOpenTransaction(request.AccountingPeriodId, request.TransactionId, entries, cancellationToken);
            await eventHandler.Handle(
                notification: new OpenTransactionAmountChanged.Event(-1),
                cancellationToken);
        }
    }
}
