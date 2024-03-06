using System.Diagnostics.CodeAnalysis;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Domain.Journal;

public class JournalEntry
{
    private readonly Account _debitAccount;
    private readonly Account _creditAccount;
    public required Guid Id { get; init; }
    public required AccountingPeriod AccountingPeriod { get; init; }
    public required DateOnly ValueDate { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required Account DebitAccount
    {
        get => _debitAccount;
        [MemberNotNull(member: nameof(_debitAccount))]
        init
        {
            ThrowIfAccountsAreSame(CreditAccount, value);
            _debitAccount = value;
        }
    }

    public required Account CreditAccount
    {
        get => _creditAccount;
        [MemberNotNull(member: nameof(_creditAccount))]
        init
        {
            ThrowIfAccountsAreSame(DebitAccount, value);
            _creditAccount = value;
        }
    }

    public required Transaction? Transaction { get; init; }

    private static void ThrowIfAccountsAreSame(Account? firstAccount, Account? secondAccount)
    {
        if (firstAccount is null) return;
        if (secondAccount is null) return;
        if (firstAccount.Id != secondAccount.Id) return;

        throw new AccountsMustNotBeSameException();
    }

    public override string ToString() =>
        $"{DebitAccount.Name} - {CreditAccount.Name}: {Amount:C}";
}