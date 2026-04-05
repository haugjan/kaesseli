using System.Collections.Immutable;
using Kaesseli.Features.Integration;
using Result = Kaesseli.Contracts.Integration.Transaction;

namespace Kaesseli.Features.Integration.TransactionQuery;

public static class GetTransactions
{
    public record Query(Guid TransactionSummaryId);

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository repo) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var transactions = await repo.GetTransactions(request.TransactionSummaryId, cancellationToken);
            return transactions.Select(t => new Result(
                Id: t.Id,
                RawText: t.RawText,
                Amount: t.Amount,
                ValueDate: t.ValueDate,
                BookDate: t.BookDate,
                Description: t.Description,
                Reference: t.Reference,
                TransactionCode: t.TransactionCode,
                TransactionCodeDetail: t.TransactionCodeDetail,
                Debtor: t.Debtor,
                Creditor: t.Creditor)).ToImmutableList();
        }
    }
}
