using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Automation;

public class AutomationEntryPart
{
    private AutomationEntryPart() { }

    public Guid Id { get; private init; }
    public Account Account { get; private init; } = null!;
    public decimal AmountProportion { get; private init; }

    public static AutomationEntryPart Create(Account account, decimal amountProportion)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new AutomationEntryPart
        {
            Id = Guid.NewGuid(),
            Account = account,
            AmountProportion = amountProportion,
        };
    }
}
