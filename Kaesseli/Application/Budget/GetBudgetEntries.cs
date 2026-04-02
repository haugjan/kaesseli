using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

public static class GetBudgetEntries
{
    public record Query(Guid? AccountId, AccountType? AccountType, Guid AccountingPeriodId);

    public record Result(Guid Id, decimal Amount, string Description, Guid AccountId, Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query query, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IBudgetRepository repository) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(Query query, CancellationToken cancellationToken)
        {
            var entries = await repository.GetBudgetEntries(
                              query.AccountingPeriodId, query.AccountId, query.AccountType,
                              cancellationToken);
            return entries.Select(entry => entry.ToGetBudgetEntriesQueryResult()).ToImmutableList();
        }
    }
}
