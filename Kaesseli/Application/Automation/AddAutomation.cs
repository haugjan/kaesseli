using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Automation;

namespace Kaesseli.Application.Automation;

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
                parts.Add(
                    item: new AutomationEntryPart
                    {
                        Id = Guid.NewGuid(),
                        Account = await GetAccount(entry.OtherAccountId, cancellationToken),
                        AmountProportion = entry.Amount / sumOfAllEntries
                    });
            }

            var automationEntry = new AutomationEntry { Id = Guid.NewGuid(), AutomationText = request.AutomationText, Parts = parts };

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
