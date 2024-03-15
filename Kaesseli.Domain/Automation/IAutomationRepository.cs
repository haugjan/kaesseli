using Kaesseli.Domain.Integration;

namespace Kaesseli.Domain.Automation;

public interface IAutomationRepository
{
    Task<int> GetNrOfPossibleAutomation(string requestAutomationText, CancellationToken cancellationToken);

    Task AddAutomation(AutomationEntry entry, CancellationToken cancellationToken);
    Task<IEnumerable<Transaction>> GetPossibleTransactions(string automationText, CancellationToken cancellationToken);
    Task<IEnumerable<AutomationEntry>> GetAutomations(CancellationToken cancellationToken);
}