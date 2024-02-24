using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using MediatR;

namespace Kaesseli.Application.Budget;

public class AddBudgetEntryCommandHandler(IBudgetRepository budgetRepository, 
                                          IAccountRepository accountRepository,
                                          IDateTimeService dateTime)
    : IRequestHandler<AddBudgetEntryCommand, Guid>
{

    public async Task<Guid> Handle(AddBudgetEntryCommand request, CancellationToken cancellationToken)
    {
        var newBudgetEntryEntity = await CreateEntityFromCommand(request, cancellationToken);
        var createdEntry = await budgetRepository.AddBudgetEntry(newBudgetEntryEntity, cancellationToken);
        return createdEntry.Id;
    }
    private async Task<BudgetEntry> CreateEntityFromCommand(AddBudgetEntryCommand addBudgetEntryCommand, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetAccount(addBudgetEntryCommand.AccountId, cancellationToken);
        var valueDate = addBudgetEntryCommand.ValueDate 
                     ?? dateTime.ToDay;
        var newBudgetEntryEntity = addBudgetEntryCommand.ToBudgetEntry(account, valueDate);
        return newBudgetEntryEntity;
    }

}