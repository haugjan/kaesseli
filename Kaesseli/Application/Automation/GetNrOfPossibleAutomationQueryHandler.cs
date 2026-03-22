using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Automation;

public interface IGetNrOfPossibleAutomationQueryHandler
{
    Task<GetNrOfPossibleAutomationQueryResult> Handle(GetNrOfPossibleAutomationQuery request, CancellationToken cancellationToken);
}

public class GetNrOfPossibleAutomationQueryHandler : IGetNrOfPossibleAutomationQueryHandler
{
    private readonly IAutomationRepository _automationRepository;

    public GetNrOfPossibleAutomationQueryHandler(IAutomationRepository automationRepository)
    {
        _automationRepository = automationRepository;
    }

    public async Task<GetNrOfPossibleAutomationQueryResult> Handle(GetNrOfPossibleAutomationQuery request, CancellationToken cancellationToken) =>
        new() { NrOfPossibleAutomation = await _automationRepository.GetNrOfPossibleAutomation(request.AutomationText, cancellationToken) };
}
