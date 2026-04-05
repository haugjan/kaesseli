namespace Kaesseli.Contracts.Features.Accounts;

public static class GetAccountsSummaryContract
{
    public record Result(
        Guid Id,
        string Name,
        string Icon,
        string IconColor,
        string Type,
        AccountType TypeId,
        decimal AccountBalance,
        decimal? Budget,
        decimal? BudgetPerMonth,
        decimal? BudgetPerYear,
        decimal? BudgetBalance,
        decimal? CurrentBudget);
}
