using System.Collections.Immutable;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.TransactionQuery;

public static class GetTransactions
{
    public record Query(Guid TransactionSummaryId);

    public record Result(
        Guid Id,
        string RawText,
        decimal Amount,
        DateOnly ValueDate,
        DateOnly BookDate,
        string Description,
        string Reference,
        string TransactionCode,
        string TransactionCodeDetail,
        string? Debtor,
        string? Creditor);

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
            return transactions.Select(transaction => transaction.ToGetTransactionSummary()).ToImmutableList();
        }
    }
}
