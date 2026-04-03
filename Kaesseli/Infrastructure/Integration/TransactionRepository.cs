using Kaesseli.Domain.Integration;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Integration;

internal class TransactionRepository : ITransactionRepository
{
    private readonly KaesseliContext _context;

    public TransactionRepository(KaesseliContext context) =>
        _context = context;

    public async Task<IEnumerable<TransactionSummary>> GetTransactionSummaries(CancellationToken cancellationToken) =>
        await _context.TransactionSummaries
                      .Include(ts => ts.Account)
                      .Include(ts => ts.Transactions)
                      .ToListAsync(cancellationToken);

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
                      .Where(tran => tran.TransactionSummary!.Id == transactionSummaryId)
                      .ToListAsync(cancellationToken);

    public async Task<Transaction?> GetNextOpenTransaction(int skip, CancellationToken cancellationToken) =>
        await _context.Transactions
                      .Include(tran => tran.TransactionSummary)
                      .Include(tran => tran.JournalEntries)
                      .Where(tran => tran.JournalEntries!.Any() == false)
                      .OrderBy(tran => tran.ValueDate)
                      .Skip(skip)
                      .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> GetTotalOpenTransaction(CancellationToken cancellationToken)
    {
        var statistic = await _context.TransactionStatistics.FirstOrDefaultAsync(cancellationToken);

        if (statistic != null) return statistic.TotalOpenTransaction;

        var totalOpen = await _context.Transactions
                                      .Where(t => t.JournalEntries!.Any() == false)
                                      .CountAsync(cancellationToken);

        statistic = new TransactionStatistic { Id = Guid.NewGuid(), TotalOpenTransaction = totalOpen };
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
        statistic.TotalOpenTransaction += notificationAmount;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private TransactionStatistic AddStatisticEntryIfNull(TransactionStatistic? statistic)
    {
        if (statistic is not null) return statistic;

        statistic = new TransactionStatistic { Id = Guid.NewGuid(), TotalOpenTransaction = 0 };
        _context.TransactionStatistics.Add(statistic);

        return statistic;
    }
}