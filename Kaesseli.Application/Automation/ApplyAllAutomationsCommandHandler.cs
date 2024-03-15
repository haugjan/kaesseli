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
        foreach (var automationEntry in automations) await ApplyAutomation(request.AccountingPeriodId, automationEntry, cancellationToken);
    }

    private async Task ApplyAutomation(Guid accountingPeriodId, AutomationEntry automationEntry, CancellationToken cancellationToken)
    {
        var transactions = await _automateRepository.GetPossibleTransactions(automationEntry.AutomationText, cancellationToken);
        foreach (var transaction in transactions)
        {
            await ApplyAutomation(accountingPeriodId,
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
        await _mediator.Send(
            request: new SplitOpenTransactionCommand
            {
                AccountingPeriodId = accountingPeriodId,
                TransactionId = transaction.Id,
                Entries = automationEntry.Parts.Select(
                    part => new SplitOpenTransactionEntry { OtherAccountId = part.Account.Id, Amount = part.Amount })
            }, cancellationToken);
    }
}