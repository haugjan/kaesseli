using MediatR;

namespace Kaesseli.Application.Budget;

public class AddBudgetEntryCommand : IRequest<Guid>
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
}