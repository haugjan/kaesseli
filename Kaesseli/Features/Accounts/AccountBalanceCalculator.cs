using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Accounts;

public static class AccountBalanceCalculator
{
    public static decimal GetAccountBalance(Account account, IEnumerable<JournalEntry> entries) =>
        entries.Select(entry => GetSignedAmount(account, entry)).Sum();

    public static decimal GetSignedAmount(Account account, JournalEntry entry)
    {
        var amount = entry.Amount;
        if (account.Type is AccountType.Liability or AccountType.Revenue) amount = -amount;

        if (entry.DebitAccount.Id == account.Id) return amount;
        if (entry.CreditAccount.Id == account.Id) return -amount;

        return 0;
    }

    public static decimal? GetBudgetPerYear(Account account, IEnumerable<BudgetEntry> entries)
    {
        var budgetEntries = entries.Where(entry => entry.Account.Id == account.Id)
                                   .Select(entry => entry.Amount);

        if (account.Type is not (AccountType.Asset or AccountType.Liability)) return budgetEntries.Sum();

        return null;
    }

    public static decimal? GetBudgetPerMonth(Account account, IEnumerable<BudgetEntry> entries) =>
        GetBudgetPerYear(account, entries) / 12;

    public static decimal? GetBudget(Account account, IEnumerable<BudgetEntry> entries, AccountingPeriod period)
    {
        var budgetPerYear = GetBudgetPerYear(account, entries);
        var totalDays = (period.ToInclusive.ToDateTime(time: default) - period.FromInclusive.ToDateTime(time: default)).TotalDays;
        return budgetPerYear / 365m * Convert.ToDecimal(totalDays);
    }

    public static decimal? GetCurrentBudget(Account account, IEnumerable<BudgetEntry> entries, AccountingPeriod accountingPeriod, DateOnly today)
    {
        var budget = GetBudget(account, entries, accountingPeriod);
        if (budget == null) return null;

        var fromPeriod = accountingPeriod.FromInclusive;
        var toPeriod = accountingPeriod.ToInclusive;
        var toToday = today > accountingPeriod.ToInclusive
                          ? accountingPeriod.ToInclusive
                          : today;
        var daysUntilToday = (int)((toToday.ToDateTime(TimeOnly.MinValue) - fromPeriod.ToDateTime(TimeOnly.MinValue)).TotalDays + 1);
        var daysWholePeriod = (int)((toPeriod.ToDateTime(TimeOnly.MinValue) - fromPeriod.ToDateTime(TimeOnly.MinValue)).TotalDays + 1);

        return Math.Round(d: budget.Value / daysWholePeriod * daysUntilToday, decimals: 2);
    }

    public static decimal? GetBudgetBalance(AccountType accountType, decimal? budget, decimal accountBalance) =>
        accountType switch
        {
            AccountType.Revenue => accountBalance - budget,
            AccountType.Expense => budget - accountBalance,
            _ => null
        };
}
