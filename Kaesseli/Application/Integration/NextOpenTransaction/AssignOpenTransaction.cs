using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public static class AssignOpenTransaction
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid AccountingPeriodId { get; init; }
        public required Guid TransactionId { get; init; }
        public required Guid OtherAccountId { get; init; }
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
        private readonly ITransactionRepository _tranRepo;
        private readonly OpenTransactionAmountChanged.IHandler _eventHandler;

        public Handler(IJournalRepository journalRepo, ITransactionRepository tranRepo, OpenTransactionAmountChanged.IHandler eventHandler)
        {
            _journalRepo = journalRepo;
            _tranRepo = tranRepo;
            _eventHandler = eventHandler;
        }

        public async Task Handle(Query request, CancellationToken cancellationToken)
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
                notification: new OpenTransactionAmountChanged.Event { Amount = -1 },
                cancellationToken);
        }
    }
}
