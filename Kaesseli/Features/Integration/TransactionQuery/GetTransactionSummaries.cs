using System.Collections.Immutable;
using Kaesseli.Features.Integration;

namespace Kaesseli.Features.Integration.TransactionQuery;

public static class GetTransactionSummaries
{
    public record Query;

    public record Result(
        Guid Id,
        string AccountName,
        DateOnly ValueDateFrom,
        DateOnly ValueDateTo,
        decimal BalanceBefore,
        decimal BalanceAfter,
        string Reference,
        int NrOfTransactions);

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository repository) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entries = await repository.GetTransactionSummaries(cancellationToken);
            return entries.Select(entry => entry.ToGetTransactionSummary()).ToImmutableList();
        }
    }
}
