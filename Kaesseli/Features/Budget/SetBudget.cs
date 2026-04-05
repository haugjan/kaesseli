using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Budget;

public static class SetBudget
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(decimal Amount, string Description, Guid AccountId, Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IBudgetRepository budgetRepository, IAccountRepository accountRepository) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var newBudgetEntryEntity = await CreateEntityFromCommand(request, cancellationToken);
            var createdEntry = await budgetRepository.SetBudget(newBudgetEntryEntity, cancellationToken);
            return createdEntry.Id;
        }

        private async Task<BudgetEntry> CreateEntityFromCommand(Query setBudgetCommand, CancellationToken cancellationToken)
        {
            var account = await accountRepository.GetAccount(setBudgetCommand.AccountId, cancellationToken);
            var accountingPeriod = await accountRepository.GetAccountingPeriod(setBudgetCommand.AccountingPeriodId, cancellationToken);

            return BudgetEntry.Create(setBudgetCommand.Description, setBudgetCommand.Amount, account, accountingPeriod);
        }
    }
}
