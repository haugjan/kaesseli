namespace Kaesseli.Features.Automation;

public static class AddAutomation
{
    public record AutomationEntryRequest(string OtherAccountShortName, decimal Amount);

    public record Query(
        string AutomationText,
        Guid AccountingPeriodId,
        IEnumerable<AutomationEntryRequest> Entries
    );

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(
        IAutomationRepository automateRepository,
        ApplyAllAutomations.IHandler applyAllAutomationsHandler
    ) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var entries = request.Entries.ToList();
            var sumOfAllEntries = entries.Sum(entry => entry.Amount);
            var parts = entries
                .Select(entry =>
                    AutomationEntryPart.Create(
                        entry.OtherAccountShortName,
                        entry.Amount / sumOfAllEntries
                    )
                )
                .ToList();

            var automationEntry = AutomationEntry.Create(request.AutomationText, parts);

            await automateRepository.AddAutomation(automationEntry, cancellationToken);

            await applyAllAutomationsHandler.Handle(
                request: new ApplyAllAutomations.Query(request.AccountingPeriodId),
                cancellationToken
            );
            return automationEntry.Id;
        }
    }
}
