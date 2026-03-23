using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Automation;

public static class ApplyAllAutomations
{
    public record Query
    {
        public required Guid AccountingPeriodId { get; init; }
    }

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        private readonly IAutomationRepository _automateRepository;
        private readonly SplitOpenTransaction.IHandler _splitHandler;

        public Handler(IAutomationRepository automateRepository, SplitOpenTransaction.IHandler splitHandler)
        {
            _automateRepository = automateRepository;
            _splitHandler = splitHandler;
        }

        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var automations = await _automateRepository.GetAutomations(cancellationToken);
            foreach (var automationEntry in automations)
                await ApplyAutomation(request.AccountingPeriodId, automationEntry, cancellationToken);
        }

        private async Task ApplyAutomation(Guid accountingPeriodId, AutomationEntry automationEntry, CancellationToken cancellationToken)
        {
            var transactions = await _automateRepository.GetPossibleTransactions(automationEntry.AutomationText, cancellationToken);
            foreach (var transaction in transactions)
            {
                await ApplyAutomation(accountingPeriodId, automationEntry, transaction, cancellationToken);
            }
        }

        private async Task ApplyAutomation(
            Guid accountingPeriodId,
            AutomationEntry automationEntry,
            Transaction transaction,
            CancellationToken cancellationToken)
        {
            var entries = automationEntry.Parts.Take(count: automationEntry.Parts.Count() - 1)
                                         .Select(
                                             part => new SplitOpenTransactionEntry
                                             {
                                                 OtherAccountId = part.Account.Id,
                                                 Amount = Math.Round(d: transaction.Amount * part.AmountProportion, decimals: 2)
                                             })
                                         .ToList();

            var lastPart = automationEntry.Parts.Last();
            var remainingAmount = transaction.Amount - entries.Sum(entry => entry.Amount);

            entries.Add(item: new SplitOpenTransactionEntry { OtherAccountId = lastPart.Account.Id, Amount = remainingAmount });

            await _splitHandler.Handle(
                request: new SplitOpenTransaction.Query
                {
                    AccountingPeriodId = accountingPeriodId,
                    TransactionId = transaction.Id,
                    Entries = entries
                },
                cancellationToken);
        }
    }
}
