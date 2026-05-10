namespace Kaesseli.Features.Integration.NextOpenTransaction;

public static class SetIgnoreTransaction
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(Guid TransactionId, bool IsIgnored);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(
        ITransactionRepository tranRepo,
        UpdateOpenTransactionTotal.IHandler updateOpenTotal
    ) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await tranRepo.GetTransaction(request.TransactionId, cancellationToken);
            if (transaction.IsIgnored == request.IsIgnored)
                return;

            var hasJournalEntries = await tranRepo.HasJournalEntries(request.TransactionId, cancellationToken);

            await tranRepo.SetTransactionIgnored(request.TransactionId, request.IsIgnored, cancellationToken);

            if (hasJournalEntries)
                return;

            var delta = request.IsIgnored ? -1 : +1;
            await updateOpenTotal.Handle(new UpdateOpenTransactionTotal.Query(delta), cancellationToken);
        }
    }
}
