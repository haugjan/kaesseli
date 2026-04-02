using Kaesseli.Application.Accounts;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Accounts;

internal static class AccountExtensions
{
    internal static GetAccountsSummary.Result ToAccountSummary(
        this Account account,
        decimal accountBalance,
        decimal? budget,
        decimal? budgetPerMonth,
        decimal? budgetPerYear,
        decimal? currentBudget,
        decimal? budgetBalance
    ) =>
        new(
            Id: account.Id,
            Name: account.Name,
            Icon: account.Icon.Name,
            IconColor: account.Icon.Color,
            Type: account.Type.DisplayName(),
            TypeId: account.Type,
            AccountBalance: accountBalance,
            Budget: budget,
            BudgetPerMonth: budgetPerMonth,
            BudgetPerYear: budgetPerYear,
            BudgetBalance: budgetBalance,
            CurrentBudget: currentBudget);
}
