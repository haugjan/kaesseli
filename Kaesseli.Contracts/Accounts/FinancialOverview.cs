namespace Kaesseli.Contracts.Accounts;

public record FinancialOverview(
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
