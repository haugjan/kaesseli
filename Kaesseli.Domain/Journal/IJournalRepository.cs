namespace Kaesseli.Domain.Journal;

public interface IJournalRepository
{
    Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity, CancellationToken cancellationToken);
    Task<IEnumerable<JournalEntry>> GetJournalEntries(GetJournalEntriesRequest request, CancellationToken cancellationToken);

    Task AssignOpenTransaction(
        Guid accountingPeriodId,
        Guid transactionId,
        IEnumerable<AssignOpenTransactionEntry> entries,
        CancellationToken cancellationToken);
}