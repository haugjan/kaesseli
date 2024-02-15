using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Budget;

public class GetBudgetEntriesRequest
{
    public Guid? AccountId { get; init; }
    public AccountType? AccountType { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public static GetBudgetEntriesRequest Empty => new() { AccountId = null, FromDate = null, ToDate = null };
}