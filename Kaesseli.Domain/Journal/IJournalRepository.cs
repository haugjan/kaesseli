using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Journal;

public interface IJournalRepository
{
    Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity, CancellationToken cancellationToken);

    Task<IEnumerable<JournalEntry>> GetJournalEntries(
        Guid accountingPeriodId, Guid? accountId, AccountType? accountType,
        CancellationToken cancellationToken);

    Task AssignOpenTransaction(
        Guid accountingPeriodId,
        Guid transactionId,
        IEnumerable<(Guid OtherAccountId, decimal Amount)> entries,
        CancellationToken cancellationToken);
}