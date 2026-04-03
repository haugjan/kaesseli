using Kaesseli.Domain.Accounts;
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
        Guid accountingPeriodId, Guid? accountId, AccountType? accountType,
        CancellationToken cancellationToken)
    {
        var entries = context.JournalEntries
                             .Include(journalEntry => journalEntry.DebitAccount)
                             .Include(journalEntry => journalEntry.CreditAccount)
                             .Include(journalEntry => journalEntry.AccountingPeriod)
                             .Where(entry => entry.AccountingPeriod.Id == accountingPeriodId);
        if (accountId is not null)
        {
            entries = entries.Where(
                journal => journal.CreditAccount.Id == accountId
                        || journal.DebitAccount.Id == accountId);
        }

        if (accountType is not null)
        {
            entries = entries.Where(
                entry => entry.DebitAccount.Type == accountType
                      || entry.CreditAccount.Type == accountType);
        }

        return await entries.ToListAsync(cancellationToken);
    }

    public async Task AssignOpenTransaction(
        Guid accountingPeriodId,
        Guid transactionId,
        IEnumerable<(Guid OtherAccountId, decimal Amount)> entries,
        CancellationToken cancellationToken)
    {
        var entriesArray = entries.ToArray();
        var transaction = await context.Transactions
                                       .Include(trans => trans.TransactionSummary)
                                       .ThenInclude(summary => summary!.Account)
                                       .SingleAsync(trans => trans.Id == transactionId, cancellationToken);
        var accountingPeriod = await context.AccountingPeriods.FirstOrDefaultAsync(ap => ap.Id == accountingPeriodId, cancellationToken)
                            ?? throw new EntityNotFoundException(entityType: typeof(AccountingPeriod), accountingPeriodId);
        WrongAmountException.ThrowIfAmountNotMatch(transaction.Amount, entriesAmount: entriesArray.Sum(entry => entry.Amount));

        foreach (var entry in entriesArray)
            await AssignOpenTransaction(
                accountingPeriod,
                transaction,
                entry.OtherAccountId,
                entry.Amount,
                cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }



    private async Task AssignOpenTransaction(
        AccountingPeriod accountingPeriod,
        Transaction transaction,
        Guid otherAccountId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var otherAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Id == otherAccountId, cancellationToken)
                        ?? throw new EntityNotFoundException(entityType: typeof(Account), otherAccountId);
        var newJournalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = transaction.ValueDate,
            Description = transaction.Description,
            Amount = amount,
            DebitAccount = transaction.TransactionSummary!.Account,
            CreditAccount = otherAccount,
            Transaction = transaction,
            AccountingPeriod = accountingPeriod
        };

        context.JournalEntries.Add(newJournalEntry);
    }
}