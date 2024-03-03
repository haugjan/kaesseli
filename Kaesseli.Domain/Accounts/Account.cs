using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using Serilog;

namespace Kaesseli.Domain.Accounts;

public class Account
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required AccountType Type { get; init; }
    public required string Icon { get; init; }
    public required string IconColor { get; init; }

    public decimal GetAccountBalance(IEnumerable<JournalEntry> entries) =>
        entries.Select(GetSignedAmount).Sum();

    public decimal GetSignedAmount(JournalEntry entry)
    {
        var amount = entry.Amount;
        if (Type is AccountType.Liability or AccountType.Revenue) amount = -amount;

        if (entry.DebitAccount.Id == Id) return amount;
        if (entry.CreditAccount.Id == Id) return -amount;

        return 0;
    }

    public decimal? GetBudget(IEnumerable<BudgetEntry> entries)
    {
        var budgetEntries = entries.Where(entry => entry.Account.Id == Id)
                                   .Select(entry => entry.Amount);
        if (Type is not (AccountType.Asset or AccountType.Liability)) return budgetEntries.Sum();

        if (budgetEntries.Any()) Log.Logger.Warning(messageTemplate: "Found budget entries on account type {AccountType}", Type);
        return null;
    }

    public decimal? GetBudgetBalance(decimal? budget, decimal accountBalance) =>
        Type switch
        {
            AccountType.Revenue => accountBalance - budget, 
            AccountType.Expense => budget - accountBalance, 
            _ => null
        };

    public override string ToString() =>
        $"{Name} ({Type.DisplayName()})";
}