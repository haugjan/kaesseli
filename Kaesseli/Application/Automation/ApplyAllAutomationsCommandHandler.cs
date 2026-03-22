using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Automation;

public interface IApplyAllAutomationsCommandHandler
{
    Task Handle(ApplyAllAutomationsCommand request, CancellationToken cancellationToken);
}

public class ApplyAllAutomationsCommandHandler : IApplyAllAutomationsCommandHandler
{
    private readonly IAutomationRepository _automateRepository;
    private readonly ISplitOpenTransactionCommandHandler _splitHandler;

    public ApplyAllAutomationsCommandHandler(IAutomationRepository automateRepository, ISplitOpenTransactionCommandHandler splitHandler)
    {
        _automateRepository = automateRepository;
        _splitHandler = splitHandler;
    }

    public async Task Handle(ApplyAllAutomationsCommand request, CancellationToken cancellationToken)
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
            await ApplyAutomation(
                accountingPeriodId,
                automationEntry,
                transaction,
                cancellationToken);
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
            request: new SplitOpenTransactionCommand
            {
                AccountingPeriodId = accountingPeriodId,
                TransactionId = transaction.Id,
                Entries = entries
            },
            cancellationToken);
    }
}
