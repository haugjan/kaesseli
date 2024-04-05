using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Automation;

public class GetNrOfPossibleAutomationQueryHandler : IRequestHandler<GetNrOfPossibleAutomationQuery,
    GetNrOfPossibleAutomationQueryResult>
{
    private readonly IAutomationRepository _automationRepository;

    public GetNrOfPossibleAutomationQueryHandler(IAutomationRepository automationRepository)
    {
        _automationRepository = automationRepository;
    }

    public async Task<GetNrOfPossibleAutomationQueryResult> Handle(GetNrOfPossibleAutomationQuery request, CancellationToken cancellationToken) =>
        new() { NrOfPossibleAutomation = await _automationRepository.GetNrOfPossibleAutomation(request.AutomationText, cancellationToken) };
}