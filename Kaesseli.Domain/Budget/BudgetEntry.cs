using System.Diagnostics.CodeAnalysis;
using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Budget;

public class BudgetEntry
{
    private readonly Account _account;
    public required Guid Id { get; init; }
    public required DateOnly ValueDate { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }

    public required Account Account
    {
        get => _account;
        [MemberNotNull(member: nameof(_account))]
        init
        {
            ThrowIfWrongAccountType(value);
            _account = value;
        }
    }

    private static void ThrowIfWrongAccountType(Account value)
    {
        if (value.Type is AccountType.Asset 
            or AccountType.Liability) 
            throw new BudgetNotAllowedException(value.Type);
    }

    public override string ToString() =>
        $"Budget {Account.Name}: {Amount:C}";
}