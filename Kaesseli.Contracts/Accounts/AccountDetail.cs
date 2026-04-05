namespace Kaesseli.Contracts.Accounts;

public record AccountDetail(
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
    decimal? CurrentBudget,
    decimal? BudgetBalance,
    IEnumerable<AccountEntry> Entries);

public record AccountEntry(
    Guid Id,
    DateOnly ValueDate,
    string Description,
    decimal Amount,
    AmountType AmountType,
    string? OtherAccount,
    Guid? OtherAccountId);

public enum AmountType
{
    Budget = 1,
    Credit = 2,
    Debit = 3
}
