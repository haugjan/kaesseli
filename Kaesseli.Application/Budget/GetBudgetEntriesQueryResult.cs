namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQueryResult
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required decimal Amount { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string Description { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required Guid AccountId { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required DateOnly ValueDate { get; init; }
}