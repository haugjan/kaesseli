using System.Collections.Immutable;
using Kaesseli.Features.Integration;
using Result = Kaesseli.Contracts.Features.Integration.TransactionQuery.GetTransactionSummariesContract.Result;

namespace Kaesseli.Features.Integration.TransactionQuery;

public static class GetTransactionSummaries
{
    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository repository) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(CancellationToken cancellationToken)
        {
            var entries = await repository.GetTransactionSummaries(cancellationToken);
            return entries.Select(entry => new Result(
                Id: entry.Id,
                AccountName: entry.Account.Name,
                ValueDateFrom: entry.ValueDateFrom,
                ValueDateTo: entry.ValueDateTo,
                BalanceBefore: entry.BalanceBefore,
                BalanceAfter: entry.BalanceAfter,
                Reference: entry.Reference,
                NrOfTransactions: entry.Transactions.Count())).ToImmutableList();
        }
    }
}
