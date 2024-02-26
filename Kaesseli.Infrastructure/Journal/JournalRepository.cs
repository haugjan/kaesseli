using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Journal;

public class JournalRepository(KaesseliContext context) : IJournalRepository
{
    public async Task<JournalEntry> AddJournalEntry(JournalEntry newJournalEntryEntity, CancellationToken cancellationToken)
    {
        context.JournalEntries.Add(newJournalEntryEntity);
        await context.SaveChangesAsync(cancellationToken);
        return newJournalEntryEntity;
    }

    public async Task<IEnumerable<JournalEntry>> GetJournalEntries(
        GetJournalEntriesRequest request,
        CancellationToken cancellationToken)
    {
        IQueryable<JournalEntry> entries = context.JournalEntries
                                                  .Include(budget => budget.DebitAccount)
                                                  .Include(budget => budget.CreditAccount);
        if (request.DebitAccountId is not null) entries = entries.Where(entry => entry.DebitAccount.Id == request.DebitAccountId);
        if (request.CreditAccountId is not null) entries = entries.Where(entry => entry.CreditAccount.Id == request.DebitAccountId);

        if (request.AccountId is not null)
        {
            entries = entries.Where(
                journal => journal.CreditAccount.Id == request.AccountId
                        || journal.DebitAccount.Id == request.AccountId);
        }

        if (request.FromDate is not null) entries = entries.Where(entry => entry.ValueDate >= request.FromDate);
        if (request.ToDate is not null) entries = entries.Where(entry => entry.ValueDate < request.ToDate);

        if (request.AccountType is not null)
        {
            entries = entries.Where(
                entry => entry.DebitAccount.Type == request.AccountType
                      || entry.CreditAccount.Type == request.AccountType);
        }

        return await entries.ToListAsync(cancellationToken);
    }

}