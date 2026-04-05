using System.Collections.Immutable;
using Kaesseli.Features.Accounts;
using Result = Kaesseli.Contracts.Budget.BudgetEntry;

namespace Kaesseli.Features.Budget;

public static class GetBudgetEntries
{
    public record Query(Guid? AccountId, AccountType? AccountType, Guid AccountingPeriodId);

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
            return entries.Select(entry => new Result(
                Id: entry.Id,
                Amount: entry.Amount,
                Description: entry.Description,
                AccountId: entry.Account.Id,
                AccountingPeriodId: entry.AccountingPeriod.Id)).ToImmutableList();
        }
    }
}
