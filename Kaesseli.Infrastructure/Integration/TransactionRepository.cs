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

    public async Task<Transaction?> GetNextOpenTransaction(int skip, CancellationToken cancellationToken)
    {
        return await _context.Transactions
                             .Include(tran=> tran.TransactionSummary)
                             .Where(
                   tran => tran.JournalEntries!.Any() == false)
                             .OrderBy(tran=> tran.ValueDate)
                             .Skip(skip)
                             .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}