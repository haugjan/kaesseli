using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Domain.Accounts;

public class Account
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required AccountType Type { get; init; }

    public decimal GetAccountBalance(IEnumerable<JournalEntry> entries)
    {
        entries = entries.ToArray();
        var debits = GetDebits(entries);
        var credits = GetCredits(entries);
        return GetBalanceDependingOnAccountType(debits, credits);
    }

    public decimal GetBudget(IEnumerable<BudgetEntry> entries) =>
        entries.Where(entry => entry.Account.Id == Id).Sum(entry => entry.Amount);

    private decimal GetBalanceDependingOnAccountType(decimal debits, decimal credits)
    {
        var balance = Type switch
        {
            AccountType.Asset => debits - credits,
            AccountType.Expense => debits - credits,
            AccountType.Liability => credits - debits,
            AccountType.Revenue => credits - debits,
            // ReSharper disable StringLiteralTypo
            _ => throw new InvalidOperationException(message: "Unbekannter Kontotyp")
            // ReSharper restore StringLiteralTypo
        };

        return balance;
    }

    private decimal GetCredits(IEnumerable<JournalEntry> entries) =>
        entries.Where(entry => entry.CreditAccount.Id == Id).Sum(entry => entry.Amount);

    private decimal GetDebits(IEnumerable<JournalEntry> entries) =>
        entries.Where(entry => entry.DebitAccount.Id == Id).Sum(entry => entry.Amount);
}