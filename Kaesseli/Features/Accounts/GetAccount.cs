using System.Collections.Immutable;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Accounts;

public static class GetAccount
{
    public record Query(Guid AccountId, Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<Contracts.Accounts.AccountStatement> Handle(
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
        public async Task<Contracts.Accounts.AccountStatement> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var account = await accountRepo.GetAccount(request.AccountId, cancellationToken);
            var period = await accountRepo.GetAccountingPeriod(
                request.AccountingPeriodId,
                cancellationToken
            );
            var journalEntries = (
                await journalRepo.GetJournalEntries(
                    request.AccountingPeriodId,
                    request.AccountId,
                    accountType: null,
                    cancellationToken
                )
            ).ToArray();
            var budgetEntries = (
                await budgetRepo.GetBudgetEntries(
                    request.AccountingPeriodId,
                    request.AccountId,
                    accountType: null,
                    cancellationToken
                )
            ).ToArray();

            var accountBalance = AccountBalanceCalculator.GetAccountBalance(
                account,
                journalEntries
            );
            var budget = AccountBalanceCalculator.GetBudget(account, budgetEntries, period);
            var budgetPerMonth = AccountBalanceCalculator.GetBudgetPerMonth(account, budgetEntries);
            var budgetPerYear = AccountBalanceCalculator.GetBudgetPerYear(account, budgetEntries);
            var currentBudget = AccountBalanceCalculator.GetCurrentBudget(
                account,
                budgetEntries,
                period,
                DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime)
            );
            var budgetBalance = AccountBalanceCalculator.GetBudgetBalance(
                account.Type,
                currentBudget,
                accountBalance
            );

            return new Contracts.Accounts.AccountStatement(
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
                CurrentBudget: currentBudget,
                BudgetBalance: budgetBalance,
                Entries: GetEntries(account.Id, journalEntries, budgetEntries)
            );
        }

        private static IEnumerable<Contracts.Accounts.AccountStatementEntry> GetEntries(
            Guid accountId,
            IEnumerable<JournalEntry> journalEntries,
            IEnumerable<BudgetEntry> budgetEntries
        )
        {
            var journalResults = journalEntries.Select(entry =>
                CreateResultEntry(accountId, entry)
            );
            var budgetResults = budgetEntries.Select(CreateResultEntry);

            return journalResults
                .Concat(budgetResults)
                .OrderBy(entry => entry.ValueDate)
                .ThenBy(entry => entry.AmountType)
                .ThenBy(entry => entry.Amount)
                .ToImmutableList();
        }

        private static Contracts.Accounts.AccountStatementEntry CreateResultEntry(
            BudgetEntry entry
        ) =>
            new(
                Id: entry.Id,
                ValueDate: entry.AccountingPeriod.FromInclusive,
                Description: entry.Description,
                Amount: entry.Amount,
                AmountType: Contracts.Accounts.AmountType.Budget,
                OtherAccount: null,
                OtherAccountId: null
            );

        private static Contracts.Accounts.AccountStatementEntry CreateResultEntry(
            Guid accountId,
            JournalEntry entry
        )
        {
            var account =
                entry.CreditAccount.Id == accountId ? entry.CreditAccount : entry.DebitAccount;
            var otherAccount =
                entry.CreditAccount.Id == accountId ? entry.DebitAccount : entry.CreditAccount;
            var isDebit = entry.DebitAccount.Id == accountId;
            var amount = AccountBalanceCalculator.GetSignedAmount(account, entry);

            var amountType =
                entry.IsOpeningBalance ? Contracts.Accounts.AmountType.OpeningBalance
                : isDebit ? Contracts.Accounts.AmountType.Debit
                : Contracts.Accounts.AmountType.Credit;

            return new Contracts.Accounts.AccountStatementEntry(
                Id: entry.Id,
                ValueDate: entry.ValueDate,
                Description: entry.Description,
                Amount: amount,
                AmountType: amountType,
                OtherAccount: otherAccount.Name,
                OtherAccountId: otherAccount.Id
            );
        }
    }
}
