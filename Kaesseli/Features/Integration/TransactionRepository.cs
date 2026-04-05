using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Features.Integration;

internal class TransactionRepository : ITransactionRepository
{
    private readonly KaesseliContext _context;

    public TransactionRepository(KaesseliContext context) =>
        _context = context;

    public async Task<IEnumerable<TransactionSummary>> GetTransactionSummaries(CancellationToken cancellationToken)
    {
        var summaries = await _context.TransactionSummaries.ToListAsync(cancellationToken);
        var accounts = await _context.Accounts.ToListAsync(cancellationToken);
        var accountMap = accounts.ToDictionary(a => a.Id);
        var transactions = await _context.Transactions.ToListAsync(cancellationToken);

        foreach (var summary in summaries)
        {
            var accountFk = _context.Entry(summary).Property<Guid>("AccountId").CurrentValue;
            if (accountMap.TryGetValue(accountFk, out var account))
                _context.Entry(summary).Reference(s => s.Account).CurrentValue = account;

            var related = transactions
                .Where(t => _context.Entry(t).Property<Guid?>("TransactionSummaryId").CurrentValue == summary.Id)
                .ToList();
            _context.Entry(summary).Collection(s => s.Transactions).CurrentValue = related;
        }

        return summaries;
    }

    public async Task<TransactionSummary> AddTransactionSummary(
        TransactionSummary transactionSummary,
        CancellationToken cancellationToken)
    {
        _context.TransactionSummaries.Add(transactionSummary);
        await _context.SaveChangesAsync(cancellationToken);
        return transactionSummary;
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid transactionSummaryId, CancellationToken cancellationToken) =>
        await _context.Transactions
                      .Where(t => EF.Property<Guid?>(t, "TransactionSummaryId") == transactionSummaryId)
                      .ToListAsync(cancellationToken);

    public async Task<Transaction?> GetNextOpenTransaction(int skip, CancellationToken cancellationToken)
    {
        var allTransactions = await _context.Transactions.ToListAsync(cancellationToken);
        var journalEntries = await _context.JournalEntries.ToListAsync(cancellationToken);

        var transactionIdsWithJournal = journalEntries
            .Select(je => _context.Entry(je).Property<Guid?>("TransactionId").CurrentValue)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var openTransaction = allTransactions
            .Where(t => !transactionIdsWithJournal.Contains(t.Id))
            .OrderBy(t => t.ValueDate)
            .Skip(skip)
            .FirstOrDefault();

        if (openTransaction == null) return null;

        var summaryId = _context.Entry(openTransaction).Property<Guid?>("TransactionSummaryId").CurrentValue;
        if (summaryId.HasValue)
        {
            var summary = await _context.TransactionSummaries
                .FirstOrDefaultAsync(s => s.Id == summaryId.Value, cancellationToken);
            if (summary != null)
                _context.Entry(openTransaction).Reference(t => t.TransactionSummary!).CurrentValue = summary;
        }

        _context.Entry(openTransaction).Collection(t => t.JournalEntries!).CurrentValue = [];

        return openTransaction;
    }

    public async Task<int> GetTotalOpenTransaction(CancellationToken cancellationToken)
    {
        var statistic = await _context.TransactionStatistics.FirstOrDefaultAsync(cancellationToken);

        if (statistic != null) return statistic.TotalOpenTransaction;

        var allTransactions = await _context.Transactions.ToListAsync(cancellationToken);
        var journalEntries = await _context.JournalEntries.ToListAsync(cancellationToken);

        var transactionIdsWithJournal = journalEntries
            .Select(je => _context.Entry(je).Property<Guid?>("TransactionId").CurrentValue)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var totalOpen = allTransactions.Count(t => !transactionIdsWithJournal.Contains(t.Id));

        statistic = TransactionStatistic.Create(totalOpen);
        _context.TransactionStatistics.Add(statistic);
        await _context.SaveChangesAsync(cancellationToken);

        return statistic.TotalOpenTransaction;
    }

    public async Task<Transaction> GetTransaction(Guid requestTransactionId, CancellationToken cancellationToken) =>
        await _context.Transactions.FirstOrDefaultAsync(t => t.Id == requestTransactionId, cancellationToken)
     ?? throw new EntityNotFoundException(entityType: typeof(Transaction), requestTransactionId);

    public async Task ChangeTotalOpenTransaction(int notificationAmount, CancellationToken cancellationToken)
    {
        var statistic = await _context.TransactionStatistics.FirstOrDefaultAsync(cancellationToken);
        statistic = AddStatisticEntryIfNull(statistic);
        statistic.ChangeTotalBy(notificationAmount);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private TransactionStatistic AddStatisticEntryIfNull(TransactionStatistic? statistic)
    {
        if (statistic is not null) return statistic;

        statistic = TransactionStatistic.Create(0);
        _context.TransactionStatistics.Add(statistic);

        return statistic;
    }
}
