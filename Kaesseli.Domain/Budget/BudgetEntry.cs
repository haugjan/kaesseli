using Kaesseli.Domain.Common;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Domain.Budget;

public class BudgetEntry
{
    public required Guid Id { get; init; }
    public required DateOnly ValueDate { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }

    public required Account Account { get; init; }
    
    
}