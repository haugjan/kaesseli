using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

public static class GetBudgetEntries
{
    public record Query
    {
        public required Guid? AccountId { get; init; }
        public required AccountType? AccountType { get; init; }
        public required Guid AccountingPeriodId { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required decimal Amount { get; init; }
        public required string Description { get; init; }
        public required Guid AccountId { get; init; }
        public required Guid AccountingPeriodId { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query query, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler : IHandler
    {
        private readonly IBudgetRepository _repository;

        public Handler(IBudgetRepository repository) =>
            _repository = repository;

        public async Task<IEnumerable<Result>> Handle(Query query, CancellationToken cancellationToken)
        {
            var entries = await _repository.GetBudgetEntries(
                              query.AccountingPeriodId, query.AccountId, query.AccountType,
                              cancellationToken);
            return entries.Select(entry => entry.ToGetBudgetEntriesQueryResult()).ToImmutableList();
        }
    }
}
