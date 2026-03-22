using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

public interface ISetBudgetCommandHandler
{
    Task<Guid> Handle(SetBudgetCommand request, CancellationToken cancellationToken);
}

public class SetBudgetCommandHandler(IBudgetRepository budgetRepository,
                                     IAccountRepository accountRepository)
    : ISetBudgetCommandHandler
{
    public async Task<Guid> Handle(SetBudgetCommand request, CancellationToken cancellationToken)
    {
        var newBudgetEntryEntity = await CreateEntityFromCommand(request, cancellationToken);
        var createdEntry = await budgetRepository.SetBudget(newBudgetEntryEntity, cancellationToken);
        return createdEntry.Id;
    }

    private async Task<BudgetEntry> CreateEntityFromCommand(SetBudgetCommand setBudgetCommand, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetAccount(setBudgetCommand.AccountId, cancellationToken);
        var accountingPeriod = await accountRepository.GetAccountingPeriod(setBudgetCommand.AccountingPeriodId, cancellationToken);

        var newBudgetEntryEntity = setBudgetCommand.ToBudgetEntry(account, accountingPeriod);
        return newBudgetEntryEntity;
    }
}
