using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;

namespace Kaesseli.Application.Budget;

public static class SetBudget
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required decimal Amount { get; init; }
        public required string Description { get; init; }
        public required Guid AccountId { get; init; }
        public required Guid AccountingPeriodId { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

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

            var newBudgetEntryEntity = setBudgetCommand.ToBudgetEntry(account, accountingPeriod);
            return newBudgetEntryEntity;
        }
    }
}
