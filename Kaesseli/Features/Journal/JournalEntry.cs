using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;

namespace Kaesseli.Features.Journal;

public class JournalEntry
{
    private JournalEntry() { }

    public Guid Id { get; private init; }
    public AccountingPeriod AccountingPeriod { get; private init; } = null!;
    public DateOnly ValueDate { get; private init; }
    public string Description { get; private init; } = null!;
    public decimal Amount { get; private init; }
    public Account DebitAccount { get; private init; } = null!;
    public Account CreditAccount { get; private init; } = null!;
    public Transaction? Transaction { get; private init; }
    public bool IsOpeningBalance { get; private init; }

    public static JournalEntry Create(
        DateOnly valueDate,
        string description,
        decimal amount,
        Account debitAccount,
        Account creditAccount,
        AccountingPeriod accountingPeriod,
        Transaction? transaction = null)
    {
        ArgumentNullException.ThrowIfNull(debitAccount);
        ArgumentNullException.ThrowIfNull(creditAccount);

        if (debitAccount.Id == creditAccount.Id)
            throw new AccountsMustNotBeSameException();

        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = valueDate,
            Description = description,
            Amount = amount,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            AccountingPeriod = accountingPeriod,
            Transaction = transaction,
        };
    }

    public static JournalEntry CreateOpeningBalance(
        string description,
        decimal amount,
        Account debitAccount,
        Account creditAccount,
        AccountingPeriod accountingPeriod)
    {
        ArgumentNullException.ThrowIfNull(debitAccount);
        ArgumentNullException.ThrowIfNull(creditAccount);

        if (debitAccount.Id == creditAccount.Id)
            throw new AccountsMustNotBeSameException();

        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = accountingPeriod.FromInclusive,
            Description = description,
            Amount = amount,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            AccountingPeriod = accountingPeriod,
            IsOpeningBalance = true,
        };
    }

    public override string ToString() =>
        $"{DebitAccount.Name} - {CreditAccount.Name}: {Amount:C}";
}
