using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Automation;

public class AutomationEntryPart
{
    public required Guid Id { get; init; }
    public required Account Account { get; init; }
    public required decimal AmountProportion { get; init; }
}