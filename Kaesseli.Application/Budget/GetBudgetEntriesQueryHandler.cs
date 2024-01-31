using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQueryHandler : IRequestHandler<GetBudgetEntriesQuery>
{
    public Task Handle(GetBudgetEntriesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}