using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Automation;

namespace Kaesseli.Application.Automation;

public static class AddAutomation
{
    public record Query
    {
        public required string AutomationText { get; init; }
        public required Guid AccountingPeriodId { get; init; }
        public required IEnumerable<SplitOpenTransactionEntry> Entries { get; init; }
    }

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        private readonly IAutomationRepository _automateRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ApplyAllAutomations.IHandler _applyAllAutomationsHandler;

        public Handler(
            IAutomationRepository automateRepository,
            IAccountRepository accountRepository,
            ApplyAllAutomations.IHandler applyAllAutomationsHandler)
        {
            _automateRepository = automateRepository;
            _accountRepository = accountRepository;
            _applyAllAutomationsHandler = applyAllAutomationsHandler;
        }

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

            await _automateRepository.AddAutomation(automationEntry, cancellationToken);

            await _applyAllAutomationsHandler.Handle(
                request: new ApplyAllAutomations.Query { AccountingPeriodId = request.AccountingPeriodId },
                cancellationToken);
            return automationEntry.Id;
        }

        private async Task<Account> GetAccount(Guid otherAccountId, CancellationToken cancellationToken) =>
            await _accountRepository.GetAccount(otherAccountId, cancellationToken);
    }
}
