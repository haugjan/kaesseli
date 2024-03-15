using MediatR;

namespace Kaesseli.Application.Automation;

public class ApplyAllAutomationsCommand : IRequest
{
    public required Guid AccountingPeriodId { get; init; }
}