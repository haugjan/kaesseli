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
        var entries = await context.JournalEntries
            .Where(e => EF.Property<Guid>(e, "AccountingPeriodId") == accountingPeriodId)
            .ToListAsync(cancellationToken);

        var accounts = context.Accounts.Local.Any()
            ? context.Accounts.Local.ToList()
            : await context.Accounts.ToListAsync(cancellationToken);
        var accountMap = accounts.ToDictionary(a => a.Id);

        var period = context.AccountingPeriods.Local.FirstOrDefault(p => p.Id == accountingPeriodId)
            ?? await context.AccountingPeriods.FirstOrDefaultAsync(p => p.Id == accountingPeriodId, cancellationToken);

        foreach (var entry in entries)
        {
            SetNavigation(entry, "DebitAccount", accountMap);
            SetNavigation(entry, "CreditAccount", accountMap);
            if (period != null)
                context.Entry(entry).Reference(e => e.AccountingPeriod).CurrentValue = period;
        }

        IEnumerable<JournalEntry> result = entries;

        if (accountId is not null)
            result = result.Where(j => j.DebitAccount.Id == accountId || j.CreditAccount.Id == accountId);

        if (accountType is not null)
            result = result.Where(e => e.DebitAccount.Type == accountType || e.CreditAccount.Type == accountType);

        return result.ToList();
    }

    public async Task AssignOpenTransaction(
        Guid accountingPeriodId,
        Guid transactionId,
        IEnumerable<(Guid OtherAccountId, decimal Amount)> entries,
        CancellationToken cancellationToken)
    {
        var entriesArray = entries.ToArray();

        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Transaction), transactionId);

        var summaryId = context.Entry(transaction).Property<Guid?>("TransactionSummaryId").CurrentValue;
        if (summaryId.HasValue)
        {
            var summary = await context.TransactionSummaries
                .FirstOrDefaultAsync(s => s.Id == summaryId.Value, cancellationToken);
            if (summary != null)
            {
                context.Entry(transaction).Reference(t => t.TransactionSummary!).CurrentValue = summary;
                var account = await context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == EF.Property<Guid>(summary, "AccountId"), cancellationToken);
                if (account != null)
                    context.Entry(summary).Reference(s => s.Account).CurrentValue = account;
            }
        }

        var accountingPeriod = await context.AccountingPeriods
            .FirstOrDefaultAsync(ap => ap.Id == accountingPeriodId, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(AccountingPeriod), accountingPeriodId);

        WrongAmountException.ThrowIfAmountNotMatch(transaction.Amount, entriesArray.Sum(entry => entry.Amount));

        foreach (var entry in entriesArray)
            await AssignOpenTransaction(accountingPeriod, transaction, entry.OtherAccountId, entry.Amount, cancellationToken);

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
                        ?? throw new EntityNotFoundException(typeof(Account), otherAccountId);
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

    private void SetNavigation(JournalEntry entry, string navigationName, Dictionary<Guid, Account> accountMap)
    {
        var fkValue = context.Entry(entry).Property<Guid>(navigationName + "Id").CurrentValue;
        if (accountMap.TryGetValue(fkValue, out var account))
            context.Entry(entry).Reference(navigationName).CurrentValue = account;
    }
}
