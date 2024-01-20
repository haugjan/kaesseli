using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kässeli.Domain.Entities;

namespace Kässeli.Domain.Repositories
{
    public interface IJournalRepository
    {
        Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity);
        Task AssignAccount(Guid journalEntryId, Guid accountId);
    }
}
