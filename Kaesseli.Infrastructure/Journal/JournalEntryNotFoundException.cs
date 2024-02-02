namespace Kaesseli.Infrastructure.Journal;

public class JournalEntryNotFoundException(Guid journalEntryId) :
    Exception(message: $"Journal entry with id {journalEntryId} not found.");