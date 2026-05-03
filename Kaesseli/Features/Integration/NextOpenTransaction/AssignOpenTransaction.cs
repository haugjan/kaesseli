using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Integration.NextOpenTransaction;

public static class AssignOpenTransaction
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(Guid AccountingPeriodId, Guid TransactionId, Guid OtherAccountId);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(
        IJournalRepository journalRepo,
        ITransactionRepository tranRepo,
        UpdateOpenTransactionTotal.IHandler updateOpenTotal
    ) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await tranRepo.GetTransaction(
                request.TransactionId,
                cancellationToken
            );
            var entries = new List<(Guid OtherAccountId, decimal Amount)>
            {
                (request.OtherAccountId, transaction.Amount),
            };
            await journalRepo.AssignOpenTransaction(
                request.AccountingPeriodId,
                request.TransactionId,
                entries,
                cancellationToken
            );
            await updateOpenTotal.Handle(
                new UpdateOpenTransactionTotal.Query(-1),
                cancellationToken
            );
        }
    }
}
