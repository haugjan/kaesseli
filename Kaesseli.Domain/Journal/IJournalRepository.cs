namespace Kaesseli.Domain.Journal;

public interface IJournalRepository
{
    Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity, CancellationToken cancellationToken);
    Task<IEnumerable<JournalEntry>> GetJournalEntries(GetJournalEntriesRequest request, CancellationToken cancellationToken);
    Task<PreJournalEntry> AddPreJournalEntry(PreJournalEntry entry, CancellationToken cancellationToken);
}