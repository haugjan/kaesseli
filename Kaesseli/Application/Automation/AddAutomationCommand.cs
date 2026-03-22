using Kaesseli.Application.Integration.NextOpenTransaction;
using MediatR;

namespace Kaesseli.Application.Automation;

public class AddAutomationCommand : IRequest<Guid>
{
    public required string AutomationText { get; init; }
    public required Guid AccountingPeriodId { get; init; }
    public required IEnumerable<SplitOpenTransactionEntry> Entries { get; init; }
}