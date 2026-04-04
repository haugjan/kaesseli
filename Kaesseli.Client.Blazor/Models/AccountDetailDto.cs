namespace Kaesseli.Client.Blazor.Models;

public record AccountDetailDto(
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
    decimal? BudgetBalance,
    IEnumerable<AccountEntryDto> Entries);

public record AccountEntryDto(
    Guid Id,
    DateOnly ValueDate,
    string Description,
    decimal Amount,
    int AmountType,
    string? OtherAccount,
    Guid? OtherAccountId);
