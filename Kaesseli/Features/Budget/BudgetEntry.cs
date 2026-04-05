using System.Diagnostics.CodeAnalysis;
using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Budget;

public class BudgetEntry
{
    public required AccountingPeriod AccountingPeriod { get; init; }
    private readonly Account _account;
    public required Guid Id { get; init; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }

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
