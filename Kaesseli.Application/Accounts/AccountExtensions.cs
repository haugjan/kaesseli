using Kaesseli.Application.Accounts;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Accounts;
internal static class AccountExtensions
{
    internal static GetAccountsSummaryQueryResult ToAccountSummary(this Account account, 
                                                                   decimal accountBalance, 
                                                                   decimal? budget,
                                                                   decimal? budgetPerMonth,
                                                                   decimal? budgetPerYear,
                                                                   decimal? currentBudget,
                                                                   decimal? budgetBalance) =>
        new()
        {
            Id = account.Id,
            Name = account.Name,
            Icon = account.Icon,
            Type = account.Type.DisplayName(),
            TypeId = account.Type,
            AccountBalance = accountBalance,
            Budget = budget,
            BudgetPerMonth = budgetPerMonth,
            BudgetPerYear = budgetPerYear,
            CurrentBudget = currentBudget,
            BudgetBalance = budgetBalance,
            IconColor = account.IconColor
        };
}