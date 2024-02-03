using MediatR;

namespace Kaesseli.Application.Budget;

public class AddBudgetEntryCommand : IRequest<Guid>
{
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required Guid AccountId { get; init; }
    public required DateOnly? ValueDate { get; init; }
}