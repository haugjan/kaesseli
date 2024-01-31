using Kaesseli.Domain.Budget;
using MediatR;

namespace Kaesseli.Application.Budget;

public class BudgetEntryCommandHandler(IBudgetRepository _budgetRepository)
    : IRequestHandler<AddBudgetEntryCommand, Guid>
{

    public async Task<Guid> Handle(AddBudgetEntryCommand request, CancellationToken cancellationToken)
    {
        var newBudgetEntryEntity = await CreateEntityFromCommand(request);
        var createdEntry = await _budgetRepository.AddBudgetEntry(newBudgetEntryEntity);
        return createdEntry.Id;
    }
    private async Task<BudgetEntry> CreateEntityFromCommand(AddBudgetEntryCommand addBudgetEntryCommand)
    {
        var account = await _budgetRepository.GetAccount(addBudgetEntryCommand.AccountId);
        var newBudgetEntryEntity = new BudgetEntry()
        {
            Amount = addBudgetEntryCommand.Amount,
            Description = addBudgetEntryCommand.Description,
            Account = account
        };
        return newBudgetEntryEntity;
    }

}