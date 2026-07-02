namespace Kaesseli.Contracts.Automation;

public record AutomationEntry(
    Guid Id,
    string AutomationText,
    IReadOnlyList<AutomationEntryPart> Parts
);

public record AutomationEntryPart(string AccountShortName, decimal AmountProportion);
