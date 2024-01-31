using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQuery : IRequest<IEnumerable<GetBudgetEntriesQueryResult>>, IRequest
{
    public Guid? AccountId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}