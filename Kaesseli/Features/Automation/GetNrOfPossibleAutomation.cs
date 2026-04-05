
namespace Kaesseli.Features.Automation;

public static class GetNrOfPossibleAutomation
{
    public record Query(string AutomationText);

    public record Result(int NrOfPossibleAutomation);

    public interface IHandler
    {
        Task<Result> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IAutomationRepository automationRepository) : IHandler
    {
        public async Task<Result> Handle(Query request, CancellationToken cancellationToken) =>
            new(NrOfPossibleAutomation: await automationRepository.GetNrOfPossibleAutomation(request.AutomationText, cancellationToken));
    }
}
