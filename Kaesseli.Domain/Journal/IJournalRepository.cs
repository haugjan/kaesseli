namespace Kaesseli.Domain.Journal;

public interface IJournalRepository
{
    Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity, CancellationToken cancellationToken);
    Task<IEnumerable<JournalEntry>> GetJournalEntries(GetJournalEntriesRequest request, CancellationToken cancellationToken);
    Task AssignOpenTransaction(Guid transactionId, Guid otherAccountId, CancellationToken cancellationToken);
}