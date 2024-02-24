namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQueryResult
{
    
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
   
    
    public required decimal Amount { get; init; }
    
    public required string Description { get; init; }
    
    public required Guid AccountId { get; init; }
    
    public required DateOnly ValueDate { get; init; }
 // ReSharper restore UnusedAutoPropertyAccessor.Global
 }