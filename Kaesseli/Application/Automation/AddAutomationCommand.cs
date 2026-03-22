using Kaesseli.Application.Integration.NextOpenTransaction;

namespace Kaesseli.Application.Automation;

public class AddAutomationCommand
{
    public required string AutomationText { get; init; }
    public required Guid AccountingPeriodId { get; init; }
    public required IEnumerable<SplitOpenTransactionEntry> Entries { get; init; }
}