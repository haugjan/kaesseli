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
        await _context.TransactionSummarys
                      .Include(ts => ts.Account)
                      .Include(ts => ts.Transactions)
                      .ToListAsync(cancellationToken);

    public async Task<TransactionSummary> AddTransactionSummary(
        TransactionSummary transactionSummary,
        CancellationToken cancellationToken)
    {
        _context.TransactionSummarys.Add(transactionSummary);
        await _context.SaveChangesAsync(cancellationToken);
        return transactionSummary;
    }
}