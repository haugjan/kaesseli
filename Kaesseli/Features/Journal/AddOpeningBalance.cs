using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Journal;

public static class AddOpeningBalance
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(decimal Amount, string Description, Guid DebitAccountId, Guid CreditAccountId, Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IJournalRepository journalRepository, IAccountRepository accountRepo)
        : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var creditAccount = await accountRepo.GetAccount(request.CreditAccountId, cancellationToken);
            var debitAccount = await accountRepo.GetAccount(request.DebitAccountId, cancellationToken);
            var accountingPeriod = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);

            var newJournalEntryEntity = JournalEntry.CreateOpeningBalance(
                request.Description, request.Amount, debitAccount, creditAccount, accountingPeriod);

            var createdEntry = await journalRepository.AddJournalEntry(newJournalEntryEntity, cancellationToken);
            return createdEntry.Id;
        }
    }
}
