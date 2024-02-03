namespace Kaesseli.Domain.Journal;

public interface IJournalRepository
{
    Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity, CancellationToken cancellationToken);
    Task AssignAccount(Guid journalId, Guid accountId, CancellationToken cancellationToken);
    Task<IEnumerable<JournalEntry>> GetJournalEntries(GetJournalEntriesRequest request, CancellationToken cancellationToken);
}