namespace Kaesseli.Contracts.Features.Budget;

public static class GetBudgetEntriesContract
{
    public record Result(Guid Id, decimal Amount, string Description, Guid AccountId, Guid AccountingPeriodId);
}
