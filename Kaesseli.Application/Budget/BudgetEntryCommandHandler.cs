using Kaesseli.Domain.Budget;
using MediatR;

namespace Kaesseli.Application.Budget;

public class BudgetEntryCommandHandler(IBudgetRepository budgetRepository)
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
        var account = await budgetRepository.GetAccount(addBudgetEntryCommand.AccountId, cancellationToken);
        var newBudgetEntryEntity = new BudgetEntry()
        {
            Amount = addBudgetEntryCommand.Amount,
            Description = addBudgetEntryCommand.Description,
            Account = account
        };
        return newBudgetEntryEntity;
    }

}