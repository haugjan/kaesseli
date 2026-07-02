namespace Kaesseli.Features.Automation;

public class AutomationEntry
{
    private AutomationEntry() { }

    public Guid Id { get; private init; }
    public string AutomationText { get; private set; } = null!;
    public IEnumerable<AutomationEntryPart> Parts { get; private set; } = null!;

    public static AutomationEntry Create(string automationText, IEnumerable<AutomationEntryPart> parts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(automationText);
        ArgumentNullException.ThrowIfNull(parts);

        return new AutomationEntry
        {
            Id = Guid.NewGuid(),
            AutomationText = automationText,
            Parts = parts,
        };
    }

    public void Update(string automationText, IEnumerable<AutomationEntryPart> parts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(automationText);
        ArgumentNullException.ThrowIfNull(parts);

        AutomationText = automationText;
        Parts = parts;
    }
}
