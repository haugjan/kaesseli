namespace Kaesseli.Client.Blazor.Models;

public record FinancialOverviewDto(
    AccountTypeSummaryDto Expense,
    AccountTypeSummaryDto Revenue,
    AccountTypeSummaryDto Liability,
    AccountTypeSummaryDto Asset);

public record AccountTypeSummaryDto(
    decimal AccountBalance,
    decimal? Budget,
    decimal? BudgetPerMonth,
    decimal? BudgetPerYear,
    decimal? CurrentBudget,
    decimal? BudgetBalance);
