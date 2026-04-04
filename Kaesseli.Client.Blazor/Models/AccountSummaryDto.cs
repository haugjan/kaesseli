namespace Kaesseli.Client.Blazor.Models;

public record AccountSummaryDto(
    Guid Id,
    string Name,
    string Icon,
    string IconColor,
    string Type,
    int TypeId,
    decimal AccountBalance,
    decimal? Budget,
    decimal? BudgetPerMonth,
    decimal? BudgetPerYear,
    decimal? CurrentBudget,
    decimal? BudgetBalance);
