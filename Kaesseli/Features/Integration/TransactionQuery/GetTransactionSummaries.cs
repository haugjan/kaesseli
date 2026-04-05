using System.Collections.Immutable;
using Kaesseli.Features.Integration;

namespace Kaesseli.Features.Integration.TransactionQuery;

public static class GetTransactionSummaries
{
    public interface IHandler
    {
        Task<IEnumerable<Contracts.Integration.TransactionSummary>> Handle(CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository repository) : IHandler
    {
        public async Task<IEnumerable<Contracts.Integration.TransactionSummary>> Handle(CancellationToken cancellationToken)
        {
            var entries = await repository.GetTransactionSummaries(cancellationToken);
            return entries.Select(entry => new Contracts.Integration.TransactionSummary(
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
