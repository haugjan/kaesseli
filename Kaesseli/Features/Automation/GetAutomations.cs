namespace Kaesseli.Features.Automation;

public static class GetAutomations
{
    public interface IHandler
    {
        Task<IEnumerable<Contracts.Automation.AutomationEntry>> Handle(
            CancellationToken cancellationToken
        );
    }

    public class Handler(IAutomationRepository repository) : IHandler
    {
        public async Task<IEnumerable<Contracts.Automation.AutomationEntry>> Handle(
            CancellationToken cancellationToken
        )
        {
            var automations = await repository.GetAutomations(cancellationToken);
            return automations
                .Select(automation => new Contracts.Automation.AutomationEntry(
                    Id: automation.Id,
                    AutomationText: automation.AutomationText,
                    Parts: automation
                        .Parts.Select(part => new Contracts.Automation.AutomationEntryPart(
                            AccountShortName: part.AccountShortName,
                            AmountProportion: part.AmountProportion
                        ))
                        .ToList()
                ))
                .ToList();
        }
    }
}
