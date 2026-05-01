namespace Kaesseli.Features.Automation;

public class AutomationEntryPart
{
    private AutomationEntryPart() { }

    public Guid Id { get; private init; }
    public string AccountShortName { get; private init; } = null!;
    public decimal AmountProportion { get; private init; }

    public static AutomationEntryPart Create(string accountShortName, decimal amountProportion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountShortName);

        return new AutomationEntryPart
        {
            Id = Guid.NewGuid(),
            AccountShortName = accountShortName,
            AmountProportion = amountProportion,
        };
    }
}
