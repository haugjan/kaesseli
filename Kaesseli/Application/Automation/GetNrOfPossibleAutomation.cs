using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Automation;

public static class GetNrOfPossibleAutomation
{
    public record Query
    {
        public required string AutomationText { get; init; }
    }

    public record Result(int NrOfPossibleAutomation);

    public interface IHandler
    {
        Task<Result> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        private readonly IAutomationRepository _automationRepository;

        public Handler(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken) =>
            new(NrOfPossibleAutomation: await _automationRepository.GetNrOfPossibleAutomation(request.AutomationText, cancellationToken));
    }
}
