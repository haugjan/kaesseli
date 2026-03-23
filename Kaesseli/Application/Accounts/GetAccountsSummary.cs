using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Accounts;

public static class GetAccountsSummary
{
    public record Query
    {
        public required Guid AccountingPeriodId { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Icon { get; set; }
        public required string IconColor { get; set; }
        public required string Type { get; init; }
        public required AccountType TypeId { get; init; }
        public required decimal AccountBalance { get; init; }
        public required decimal? Budget { get; init; }
        public required decimal? BudgetPerMonth { get; init; }
        public required decimal? BudgetPerYear { get; init; }
        public required decimal? BudgetBalance { get; init; }
        public required decimal? CurrentBudget { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(
        IAccountRepository accountRepo,
        IJournalRepository journalRepo,
        IBudgetRepository budgetRepo,
        IDateTimeService dateTimeService) : IHandler
    {
        private readonly IDateTimeService _dateTimeService = dateTimeService;

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var accounts = await accountRepo.GetAccounts(cancellationToken);
            var accountingPeriod = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);

            var journalEntries = await journalRepo.GetJournalEntries(
                                     request.AccountingPeriodId, accountId: null, accountType: null,
                                     cancellationToken);

            var budgetEntries = await budgetRepo.GetBudgetEntries(
                                    request.AccountingPeriodId, accountId: null, accountType: null,
                                    cancellationToken);

            return accounts.Select(account => GetAccountSummary(account, journalEntries, budgetEntries, accountingPeriod));
        }

        private Result GetAccountSummary(
            Account account,
            IEnumerable<JournalEntry> journalEntries,
            IEnumerable<BudgetEntry> budgetEntries,
            AccountingPeriod accountingPeriod)
        {
            var today = _dateTimeService.ToDay;
            budgetEntries = budgetEntries.ToArray();
            var accountBalance = AccountBalanceCalculator.GetAccountBalance(account, journalEntries);
            var budgetPerYear = AccountBalanceCalculator.GetBudgetPerYear(account, budgetEntries);
            var budget = AccountBalanceCalculator.GetBudget(account, budgetEntries, accountingPeriod);
            var budgetPerMonth = AccountBalanceCalculator.GetBudgetPerMonth(account, budgetEntries);
            var currentBudget = AccountBalanceCalculator.GetCurrentBudget(account, budgetEntries, accountingPeriod, today);
            var budgetBalance = AccountBalanceCalculator.GetBudgetBalance(account.Type, currentBudget, accountBalance);

            return account.ToAccountSummary(accountBalance, budget, budgetPerMonth, budgetPerYear, currentBudget, budgetBalance);
        }
    }
}
