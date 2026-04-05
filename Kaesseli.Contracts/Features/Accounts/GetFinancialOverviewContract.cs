namespace Kaesseli.Contracts.Features.Accounts;

public static class GetFinancialOverviewContract
{
    public record Result(
        AccountTypeSummary Expense,
        AccountTypeSummary Revenue,
        AccountTypeSummary Liability,
        AccountTypeSummary Asset);

    public record AccountTypeSummary(
        decimal AccountBalance,
        decimal? Budget,
        decimal? BudgetPerMonth,
        decimal? BudgetPerYear,
        decimal? CurrentBudget,
        decimal? BudgetBalance);
}
