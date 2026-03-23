using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public static class SplitOpenTransaction
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid AccountingPeriodId { get; init; }
        public required Guid TransactionId { get; init; }
        public required IEnumerable<SplitOpenTransactionEntry> Entries { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler : IHandler
    {
        private readonly IJournalRepository _journalRepo;
        private readonly OpenTransactionAmountChanged.IHandler _eventHandler;

        public Handler(IJournalRepository journalRepo, OpenTransactionAmountChanged.IHandler eventHandler)
        {
            _journalRepo = journalRepo;
            _eventHandler = eventHandler;
        }

        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var entries = request.Entries.Select(entry => (entry.OtherAccountId, entry.Amount));
            await _journalRepo.AssignOpenTransaction(request.AccountingPeriodId, request.TransactionId, entries, cancellationToken);
            await _eventHandler.Handle(
                notification: new OpenTransactionAmountChanged.Event { Amount = -1 },
                cancellationToken);
        }
    }
}
