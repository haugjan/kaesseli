using MediatR;

namespace Kaesseli.Application.Automation;

public class GetNrOfPossibleAutomationQuery : IRequest<GetNrOfPossibleAutomationQueryResult>
{
    public required string AutomationText { get; init; }
}