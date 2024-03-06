using Kaesseli.Domain.Accounts;
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
                                                  .Include(journalEntry => journalEntry.DebitAccount)
                                                  .Include(journalEntry => journalEntry.CreditAccount)
                                                  .Include(journalEntry => journalEntry.AccountingPeriod);
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

    public async Task AssignOpenTransaction(
        Guid transactionId,
        Guid otherAccountId,
        Guid accountingPeriodId,
        CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
                                       .Include(trans => trans.TransactionSummary)
                                       .ThenInclude(summary => summary!.Account)
                                       .SingleAsync(trans => trans.Id == transactionId, cancellationToken);
        var otherAccount = await context.Accounts.FindAsync(otherAccountId, cancellationToken)
                        ?? throw new EntityNotFoundException(entityType: typeof(Account), otherAccountId);

        var accountingPeriod = await context.AccountingPeriods.FindAsync(accountingPeriodId, cancellationToken)
                            ?? throw new EntityNotFoundException(entityType: typeof(AccountingPeriod), accountingPeriodId);

        var newJournalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = transaction.ValueDate,
            Description = transaction.Description,
            Amount = transaction.Amount,
            DebitAccount = transaction.TransactionSummary!.Account,
            CreditAccount = otherAccount,
            Transaction = transaction,
            AccountingPeriod = accountingPeriod
        };

        context.JournalEntries.Add(newJournalEntry);
        await context.SaveChangesAsync(cancellationToken);
    }
}