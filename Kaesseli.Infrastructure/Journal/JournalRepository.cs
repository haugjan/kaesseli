using Kaesseli.Domain.Common;
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

    public async Task AssignAccount(Guid journalId, Guid accountId, CancellationToken cancellationToken)
    {
        var account = await GetAccount(accountId, cancellationToken);

        var journalEntry = await GetJournalEntry(journalId, cancellationToken);

        journalEntry.Account = account;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetJournalEntries(
        GetJournalEntriesRequest request,
        CancellationToken cancellationToken)
    {
        IQueryable<JournalEntry> entries = context.JournalEntries.Include(budget => budget.Account);
        if (request.AccountId != null) entries = entries.Where(entry => entry.Account != null && entry.Account.Id == request.AccountId);
        if (request.FromDate is not null) entries = entries.Where(entry => entry.ValueDate >= request.FromDate);
        if (request.ToDate is not null) entries = entries.Where(entry => entry.ValueDate < request.ToDate);

        return await entries.ToListAsync(cancellationToken);
    }

    private async Task<JournalEntry> GetJournalEntry(Guid journalId, CancellationToken cancellationToken) =>
        await context.JournalEntries
                     .FirstOrDefaultAsync(j => j.Id == journalId, cancellationToken)
     ?? throw new JournalEntryNotFoundException(journalId);

    private async Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken) =>
        await context.Accounts
                     .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken)
     ?? throw new AccountNotFoundException(accountId);
}