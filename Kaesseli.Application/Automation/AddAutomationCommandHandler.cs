using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Automation;
using MediatR;

namespace Kaesseli.Application.Automation;

public class AddAutomationCommandHandler : IRequestHandler<AddAutomationCommand, Guid>
{
    private readonly IAutomationRepository _automateRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMediator _mediator;

    public AddAutomationCommandHandler(
        IAutomationRepository automateRepository,
        IAccountRepository accountRepository,
        IMediator mediator)
    {
        _automateRepository = automateRepository;
        _accountRepository = accountRepository;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(AddAutomationCommand request, CancellationToken cancellationToken)
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

        var partsTasks = request.Entries.Select(
            async entry =>
                new AutomationEntryPart
                {
                    Account = await GetAccount(
                                  entry.OtherAccountId,
                                  cancellationToken),
                    AmountProportion = entry.Amount,
                    Id = Guid.NewGuid()
                });

        //var parts = await Task.WhenAll(partsTasks);
        var automationEntry = new AutomationEntry { Id = Guid.NewGuid(), AutomationText = request.AutomationText, Parts = parts };

        await _automateRepository.AddAutomation(
            automationEntry,
            cancellationToken);

        await _mediator.Send(
            request: new ApplyAllAutomationsCommand { AccountingPeriodId = request.AccountingPeriodId },
            cancellationToken);
        return automationEntry.Id;
    }

    private async Task<Account> GetAccount(Guid otherAccountId, CancellationToken cancellationToken) =>
        await _accountRepository.GetAccount(otherAccountId, cancellationToken);
}