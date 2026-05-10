using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Accounts;

public static class CheckAccountBalance
{
    public record Query(Guid AccountId, Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<Contracts.Accounts.AccountBalanceCheck> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(
        IAccountRepository accountRepo,
        IJournalRepository journalRepo,
        ITransactionRepository transactionRepo
    ) : IHandler
    {
        public async Task<Contracts.Accounts.AccountBalanceCheck> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var account = await accountRepo.GetAccount(request.AccountId, cancellationToken);
            var journalEntries = await journalRepo.GetJournalEntries(
                request.AccountingPeriodId,
                request.AccountId,
                accountType: null,
                cancellationToken);
            var currentBalance = AccountBalanceCalculator.GetAccountBalance(account, journalEntries);

            var summaries = await transactionRepo.GetTransactionSummaries(cancellationToken);
            var latest = summaries
                .Where(s => s.Account.Id == request.AccountId)
                .OrderByDescending(s => s.ValueDateTo)
                .FirstOrDefault();

            return new Contracts.Accounts.AccountBalanceCheck(
                AccountId: request.AccountId,
                CurrentBalance: currentBalance,
                LatestStatementBalance: latest?.BalanceAfter,
                LatestStatementDate: latest?.ValueDateTo,
                Difference: latest is not null ? currentBalance - latest.BalanceAfter : null);
        }
    }
}
