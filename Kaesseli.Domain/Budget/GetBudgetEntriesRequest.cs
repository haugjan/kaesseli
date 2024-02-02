namespace Kaesseli.Domain.Budget;

public class GetBudgetEntriesRequest
{
    public Guid? AccountId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}