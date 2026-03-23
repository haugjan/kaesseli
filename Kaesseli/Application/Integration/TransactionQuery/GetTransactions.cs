using System.Collections.Immutable;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.TransactionQuery;

public static class GetTransactions
{
    public record Query
    {
        public required Guid TransactionSummaryId { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required string RawText { get; init; }
        public required decimal Amount { get; init; }
        public required DateOnly ValueDate { get; init; }
        public required DateOnly BookDate { get; init; }
        public required string Description { get; init; }
        public required string Reference { get; init; }
        public required string TransactionCode { get; init; }
        public required string TransactionCodeDetail { get; init; }
        public required string? Debtor { get; init; }
        public required string? Creditor { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler : IHandler
    {
        private readonly ITransactionRepository _repo;

        public Handler(ITransactionRepository repo) =>
            _repo = repo;

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var transactions = await _repo.GetTransactions(request.TransactionSummaryId, cancellationToken);
            return transactions.Select(transaction => transaction.ToGetTransactionSummary()).ToImmutableList();
        }
    }
}
