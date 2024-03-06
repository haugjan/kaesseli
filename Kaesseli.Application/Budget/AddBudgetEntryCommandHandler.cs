using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using MediatR;

namespace Kaesseli.Application.Budget;

public class AddBudgetEntryCommandHandler(IBudgetRepository budgetRepository, 
                                          IAccountRepository accountRepository)
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
        var accountingPeriod = await accountRepository.GetAccountingPeriod(addBudgetEntryCommand.AccountingPeriodId, cancellationToken);
        
        var newBudgetEntryEntity = addBudgetEntryCommand.ToBudgetEntry(account, accountingPeriod);
        return newBudgetEntryEntity;
    }

}