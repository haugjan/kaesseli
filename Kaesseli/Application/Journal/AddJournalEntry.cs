using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Journal;

public static class AddJournalEntry
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required decimal Amount { get; init; }
        public required string Description { get; init; }
        public required DateOnly? ValueDate { get; init; }
        public required Guid DebitAccountId { get; init; }
        public required Guid CreditAccountId { get; init; }
        public required Guid AccountingPeriodId { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IJournalRepository journalRepository, IAccountRepository accountRepo, IDateTimeService dateTime)
        : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var valueDate = request.ValueDate ?? dateTime.ToDay;
            var creditAccount = await accountRepo.GetAccount(request.CreditAccountId, cancellationToken);
            var debitAccount = await accountRepo.GetAccount(request.DebitAccountId, cancellationToken);
            var accountingPeriod = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);

            var newJournalEntryEntity = request.ToJournalEntry(valueDate, debitAccount, creditAccount, accountingPeriod);

            var createdEntry = await journalRepository.AddJournalEntry(newJournalEntryEntity, cancellationToken);
            return createdEntry.Id;
        }
    }
}
