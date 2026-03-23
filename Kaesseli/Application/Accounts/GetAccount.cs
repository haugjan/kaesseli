using System.Collections.Immutable;
using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Accounts;

public static class GetAccount
{
    public record Query
    {
        public required Guid AccountId { get; init; }
        public required Guid AccountingPeriodId { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Icon { get; init; }
        public required string IconColor { get; init; }
        public required string Type { get; init; }
        public required AccountType TypeId { get; init; }
        // ReSharper disable once UnusedMember.Global
        public required decimal AccountBalance { get; init; }
        public required decimal? Budget { get; init; }
        public required decimal? BudgetPerMonth { get; init; }
        public required decimal? BudgetPerYear { get; init; }
        public required decimal? CurrentBudget { get; init; }
        public required decimal? BudgetBalance { get; init; }
        public required IEnumerable<ResultEntry> Entries { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public class ResultEntry
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required DateOnly ValueDate { get; init; }
        public required string Description { get; init; }
        public required decimal Amount { get; init; }
        public required AmountType AmountType { get; init; }
        public required string? OtherAccount { get; init; }
        public required Guid? OtherAccountId { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<Result> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository accountRepo, IJournalRepository journalRepo, IBudgetRepository budgetRepo,
                        IDateTimeService dateTimeService)
        : IHandler
    {
        private readonly IDateTimeService _dateTimeService = dateTimeService;

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await accountRepo.GetAccount(request.AccountId, cancellationToken);
            var period = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);
            var journalEntries = (await journalRepo.GetJournalEntries(
                                      request.AccountingPeriodId, request.AccountId, accountType: null,
                                      cancellationToken)).ToArray();
            var budgetEntries = (await budgetRepo.GetBudgetEntries(
                                     request.AccountingPeriodId, request.AccountId, accountType: null,
                                     cancellationToken)).ToArray();

            var accountBalance = AccountBalanceCalculator.GetAccountBalance(account, journalEntries);
            var budget = AccountBalanceCalculator.GetBudget(account, budgetEntries, period);
            var budgetPerMonth = AccountBalanceCalculator.GetBudgetPerMonth(account, budgetEntries);
            var budgetPerYear = AccountBalanceCalculator.GetBudgetPerYear(account, budgetEntries);
            var currentBudget = AccountBalanceCalculator.GetCurrentBudget(account, budgetEntries, period, _dateTimeService.ToDay);
            var budgetBalance = AccountBalanceCalculator.GetBudgetBalance(account.Type, currentBudget, accountBalance);

            return new Result
            {
                Id = account.Id,
                Name = account.Name,
                Icon = account.Icon.Name,
                Type = account.Type.DisplayName(),
                TypeId = account.Type,
                AccountBalance = accountBalance,
                Budget = budget,
                BudgetPerMonth = budgetPerMonth,
                BudgetPerYear = budgetPerYear,
                BudgetBalance = budgetBalance,
                Entries = GetEntries(account.Id, journalEntries, budgetEntries),
                IconColor = account.Icon.Color,
                CurrentBudget = currentBudget
            };
        }

        private static IEnumerable<ResultEntry> GetEntries(
            Guid accountId,
            IEnumerable<JournalEntry> journalEntries,
            IEnumerable<BudgetEntry> budgetEntries)
        {
            var journalResults = journalEntries.Select(entry => CreateResultEntry(accountId, entry));
            var budgetResults = budgetEntries.Select(CreateResultEntry);

            return journalResults.Concat(budgetResults)
                                 .OrderBy(entry => entry.ValueDate)
                                 .ThenBy(entry => entry.AmountType)
                                 .ThenBy(entry => entry.Amount)
                                 .ToImmutableList();
        }

        private static ResultEntry CreateResultEntry(BudgetEntry entry) =>
            new()
            {
                Id = entry.Id,
                ValueDate = entry.AccountingPeriod.FromInclusive,
                Description = entry.Description,
                Amount = entry.Amount,
                AmountType = AmountType.Budget,
                OtherAccount = null,
                OtherAccountId = null
            };

        private static ResultEntry CreateResultEntry(Guid accountId, JournalEntry entry)
        {
            var account = entry.CreditAccount.Id == accountId
                              ? entry.CreditAccount
                              : entry.DebitAccount;
            var otherAccount = entry.CreditAccount.Id == accountId
                                   ? entry.DebitAccount
                                   : entry.CreditAccount;
            var isDebit = entry.DebitAccount.Id == accountId;
            var amount = AccountBalanceCalculator.GetSignedAmount(account, entry);

            return new ResultEntry
            {
                Id = entry.Id,
                ValueDate = entry.ValueDate,
                Description = entry.Description,
                Amount = amount,
                AmountType = isDebit ? AmountType.Debit : AmountType.Credit,
                OtherAccount = otherAccount.Name,
                OtherAccountId = otherAccount.Id
            };
        }
    }
}
