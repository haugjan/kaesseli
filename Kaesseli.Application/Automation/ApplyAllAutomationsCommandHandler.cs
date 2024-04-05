using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Automation;

public class ApplyAllAutomationsCommandHandler : IRequestHandler<ApplyAllAutomationsCommand>
{
    private readonly IAutomationRepository _automateRepository;
    private readonly IMediator _mediator;

    public ApplyAllAutomationsCommandHandler(IAutomationRepository automateRepository, IMediator mediator)
    {
        _automateRepository = automateRepository;
        _mediator = mediator;
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
        var totalAmount = transaction;
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

        await _mediator.Send(
            request: new SplitOpenTransactionCommand
            {
                AccountingPeriodId = accountingPeriodId,
                TransactionId = transaction.Id,
                Entries = entries
            },
            cancellationToken);
    }
}