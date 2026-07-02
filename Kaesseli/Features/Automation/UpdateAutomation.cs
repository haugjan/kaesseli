namespace Kaesseli.Features.Automation;

public static class UpdateAutomation
{
    public record PartRequest(string AccountShortName, decimal AmountProportion);

    public record Query(Guid Id, string AutomationText, IEnumerable<PartRequest> Parts);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IAutomationRepository repository) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var requestedParts = request.Parts.ToList();
            var sumOfProportions = requestedParts.Sum(part => part.AmountProportion);
            var parts = requestedParts
                .Select(part =>
                    AutomationEntryPart.Create(
                        part.AccountShortName,
                        sumOfProportions == 0 ? 0 : part.AmountProportion / sumOfProportions
                    )
                )
                .ToList();

            await repository.UpdateAutomation(
                request.Id,
                request.AutomationText,
                parts,
                cancellationToken
            );
        }
    }
}
