namespace Kaesseli.Contracts.Accounts;

public record AccountOverview(
    Guid Id,
    string Name,
    string Number,
    string ShortName,
    string Icon,
    string IconColor,
    string Type,
    AccountType TypeId,
    decimal AccountBalance,
    decimal? Budget,
    decimal? BudgetPerMonth,
    decimal? BudgetPerYear,
    decimal? BudgetBalance,
    decimal? CurrentBudget
);
