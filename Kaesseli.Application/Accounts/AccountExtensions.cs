using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Accounts;
internal static class AccountExtensions
{
    internal static GetAccountsSummaryQueryResult ToAccountSummary(this Account account, decimal accountBalance, decimal budget,
                                                                   decimal budgetBalance) =>
        new()
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type.DisplayName(),
            TypeId = account.Type,
            AccountBalance = accountBalance,
            Budget = budget,
            BudgetBalance = budgetBalance
        };
}