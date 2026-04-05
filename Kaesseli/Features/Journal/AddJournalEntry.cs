using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Journal;

public static class AddJournalEntry
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(decimal Amount, string Description, DateOnly? ValueDate, Guid DebitAccountId, Guid CreditAccountId, Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IJournalRepository journalRepository, IAccountRepository accountRepo, TimeProvider timeProvider)
        : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var valueDate = request.ValueDate ?? DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
            var creditAccount = await accountRepo.GetAccount(request.CreditAccountId, cancellationToken);
            var debitAccount = await accountRepo.GetAccount(request.DebitAccountId, cancellationToken);
            var accountingPeriod = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);

            var newJournalEntryEntity = request.ToJournalEntry(valueDate, debitAccount, creditAccount, accountingPeriod);

            var createdEntry = await journalRepository.AddJournalEntry(newJournalEntryEntity, cancellationToken);
            return createdEntry.Id;
        }
    }
}
