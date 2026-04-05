namespace Kaesseli.Contracts.Features.Accounts;

public static class GetAccountContract
{
    public enum AmountType
    {
        Budget = 1,
        Credit = 2,
        Debit = 3
    }

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
        decimal? CurrentBudget,
        decimal? BudgetBalance,
        IEnumerable<ResultEntry> Entries);

    public record ResultEntry(
        Guid Id,
        DateOnly ValueDate,
        string Description,
        decimal Amount,
        AmountType AmountType,
        string? OtherAccount,
        Guid? OtherAccountId);
}
