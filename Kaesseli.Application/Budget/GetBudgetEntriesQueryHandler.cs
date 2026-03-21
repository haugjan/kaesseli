using System.Collections.Immutable;
using Kaesseli.Domain.Budget;
using MediatR;

namespace Kaesseli.Application.Budget;

// ReSharper disable once UnusedType.Global
public class GetBudgetEntriesQueryHandler :
    IRequestHandler<GetBudgetEntriesQuery, IEnumerable<GetBudgetEntriesQueryResult>>
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