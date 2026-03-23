using System.Collections.Immutable;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.TransactionQuery;

public static class GetTransactionSummaries
{
    public record Query;

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required string AccountName { get; init; }
        public required DateOnly ValueDateFrom { get; init; }
        public required DateOnly ValueDateTo { get; init; }
        public required decimal BalanceBefore { get; init; }
        public required decimal BalanceAfter { get; init; }
        public required string Reference { get; init; }
        public required int NrOfTransactions { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler : IHandler
    {
        private readonly ITransactionRepository _repository;

        public Handler(ITransactionRepository repository) =>
            _repository = repository;

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entries = await _repository.GetTransactionSummaries(cancellationToken);
            return entries.Select(entry => entry.ToGetTransactionSummary()).ToImmutableList();
        }
    }
}
