using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Automation;

public static class AddAutomation
{
    public record Query(string AutomationText, Guid AccountingPeriodId, IEnumerable<SplitOpenTransactionEntry> Entries);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(
        IAutomationRepository automateRepository,
        IAccountRepository accountRepository,
        ApplyAllAutomations.IHandler applyAllAutomationsHandler) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var parts = new List<AutomationEntryPart>();
            var sumOfAllEntries = request.Entries.Sum(entry => entry.Amount);
            foreach (var entry in request.Entries)
            {
                var account = await GetAccount(entry.OtherAccountId, cancellationToken);
                parts.Add(AutomationEntryPart.Create(account, entry.Amount / sumOfAllEntries));
            }

            var automationEntry = AutomationEntry.Create(request.AutomationText, parts);

            await automateRepository.AddAutomation(automationEntry, cancellationToken);

            await applyAllAutomationsHandler.Handle(
                request: new ApplyAllAutomations.Query(request.AccountingPeriodId),
                cancellationToken);
            return automationEntry.Id;
        }

        private async Task<Account> GetAccount(Guid otherAccountId, CancellationToken cancellationToken) =>
            await accountRepository.GetAccount(otherAccountId, cancellationToken);
    }
}
