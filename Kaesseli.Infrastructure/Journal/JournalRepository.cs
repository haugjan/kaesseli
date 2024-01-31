using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kaesseli.Domain.Journal;
using Kaesseli.Domain.Common;

namespace Kaesseli.Infrastructure.Journal
{
    public class JournalRepository : IJournalRepository
    {
        public Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity)
        {
            throw new NotImplementedException();
        }

        public Task AssignAccount(Guid JournalId, Guid accountId)
        {
            throw new NotImplementedException();
        }

        public Task<Account> GetAccount(Guid accountId)
        {
            throw new NotImplementedException();
        }
    }
}
