namespace Kaesseli.Domain.Budget;

public class GetBudgetEntriesRequest
{
    public required Guid? AccountId { get; init; }
    public required DateOnly? FromDate { get; init; }
    public required DateOnly? ToDate { get; init; }
}