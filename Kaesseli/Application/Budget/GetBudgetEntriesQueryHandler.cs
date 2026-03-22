using System.Collections.Immutable;
using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

public interface IGetBudgetEntriesQueryHandler
{
    Task<IEnumerable<GetBudgetEntriesQueryResult>> Handle(GetBudgetEntriesQuery query, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class GetBudgetEntriesQueryHandler : IGetBudgetEntriesQueryHandler
{
    private readonly IBudgetRepository _repository;

    public GetBudgetEntriesQueryHandler(IBudgetRepository repository) =>
        _repository = repository;

    public async Task<IEnumerable<GetBudgetEntriesQueryResult>> Handle(
        GetBudgetEntriesQuery query,
        CancellationToken cancellationToken)
    {
        var entries = await _repository.GetBudgetEntries(
                          query.AccountingPeriodId, query.AccountId, query.AccountType,
                          cancellationToken);
        return entries.Select(
                          entry => entry.ToGetBudgetEntriesQueryResult())
                      .ToImmutableList();
    }
}
