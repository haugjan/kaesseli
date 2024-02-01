using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQuery : IRequest<IEnumerable<GetBudgetEntriesQueryResult>>
{

    public Guid? AccountId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}