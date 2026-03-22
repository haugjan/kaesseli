namespace Kaesseli.Application.Budget;

// ReSharper disable once ClassNeverInstantiated.Global
public class SetBudgetCommand
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required Guid AccountId { get; init; }
    public required Guid AccountingPeriodId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}