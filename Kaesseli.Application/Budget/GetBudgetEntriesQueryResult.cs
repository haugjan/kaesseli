using System.Collections;
using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQueryResult
{
    public required decimal Amount { get; init; }
    public required string Description { get; init; } = string.Empty;
    public required Guid AccountId { get; init; }
}