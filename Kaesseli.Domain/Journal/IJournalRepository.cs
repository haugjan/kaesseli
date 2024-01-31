using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaesseli.Domain.Journal
{
    public interface IJournalRepository
    {
        Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity);
        Task AssignAccount(Guid journalEntryId, Guid accountId);
    }
}
