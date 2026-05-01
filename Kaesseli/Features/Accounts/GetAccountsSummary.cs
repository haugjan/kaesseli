using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Accounts;

public static class GetAccountsSummary
{
    public record Query(Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<IEnumerable<Contracts.Accounts.AccountOverview>> Handle(
            Query request,
            CancellationToken cancellationToken
        );
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(
        IAccountRepository accountRepo,
        IJournalRepository journalRepo,
        IBudgetRepository budgetRepo,
        TimeProvider timeProvider
    ) : IHandler
    {
        public async Task<IEnumerable<Contracts.Accounts.AccountOverview>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var accounts = await accountRepo.GetAccounts(cancellationToken);
            var accountingPeriod = await accountRepo.GetAccountingPeriod(
                request.AccountingPeriodId,
                cancellationToken
            );

            var journalEntries = await journalRepo.GetJournalEntries(
                request.AccountingPeriodId,
                accountId: null,
                accountType: null,
                cancellationToken
            );
            var budgetEntries = await budgetRepo.GetBudgetEntries(
                request.AccountingPeriodId,
                accountId: null,
                accountType: null,
                cancellationToken
            );

            return accounts.Select(account =>
                GetAccountOverview(account, journalEntries, budgetEntries, accountingPeriod)
            );
        }

        private Contracts.Accounts.AccountOverview GetAccountOverview(
            Account account,
            IEnumerable<JournalEntry> journalEntries,
            IEnumerable<BudgetEntry> budgetEntries,
            AccountingPeriod accountingPeriod
        )
        {
            var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
            budgetEntries = budgetEntries.ToArray();
            var accountBalance = AccountBalanceCalculator.GetAccountBalance(
                account,
                journalEntries
            );
            var budgetPerYear = AccountBalanceCalculator.GetBudgetPerYear(account, budgetEntries);
            var budget = AccountBalanceCalculator.GetBudget(
                account,
                budgetEntries,
                accountingPeriod
            );
            var budgetPerMonth = AccountBalanceCalculator.GetBudgetPerMonth(account, budgetEntries);
            var currentBudget = AccountBalanceCalculator.GetCurrentBudget(
                account,
                budgetEntries,
                accountingPeriod,
                today
            );
            var budgetBalance = AccountBalanceCalculator.GetBudgetBalance(
                account.Type,
                currentBudget,
                accountBalance
            );

            return new Contracts.Accounts.AccountOverview(
                Id: account.Id,
                Name: account.Name,
                Number: account.Number,
                ShortName: account.ShortName,
                Icon: account.Icon.Name,
                IconColor: account.Icon.Color,
                Type: account.Type.DisplayName(),
                TypeId: account.Type,
                AccountBalance: accountBalance,
                Budget: budget,
                BudgetPerMonth: budgetPerMonth,
                BudgetPerYear: budgetPerYear,
                BudgetBalance: budgetBalance,
                CurrentBudget: currentBudget
            );
        }
    }
}
