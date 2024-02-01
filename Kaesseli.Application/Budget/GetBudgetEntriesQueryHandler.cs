using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQueryHandler : 
    IRequestHandler<GetBudgetEntriesQuery, IEnumerable<GetBudgetEntriesQueryResult>>
{
    public Task<IEnumerable<GetBudgetEntriesQueryResult>> Handle(GetBudgetEntriesQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Enumerable.Range(1, 5).Select(i => new GetBudgetEntriesQueryResult
            {
                Amount = i,
                Description = i.ToString(),
                AccountId = Guid.NewGuid()
            }));
    }
}