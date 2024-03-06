using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Prediction;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Prediction;

internal class PredictionRepository : IPredictionRepository
{
    private readonly KaesseliContext _context;

    public PredictionRepository(KaesseliContext context) =>
        _context = context;

    public async Task AddLearnedPrediction(Guid transactionId, Guid accountId, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId, cancellationToken)
                       ?? throw new EntityNotFoundException(entityType: typeof(Transaction), transactionId);
        var account = await _context.Accounts.FindAsync(accountId, cancellationToken)
                   ?? throw new EntityNotFoundException(entityType: typeof(Account), accountId);

        _context.LearnedPredictions.Add(
            entity: new LearnedPrediction
            {
                Id = Guid.NewGuid(),
                AccountName = account.Name,
                Description = transaction.Description,
                Amount = transaction.Amount,
                BookDate = transaction.BookDate,
                ValueDate = transaction.ValueDate,
                Debtor = transaction.Debtor,
                Creditor = transaction.Creditor
            });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<LearnedPrediction>> GetAllLearnedPredictions(CancellationToken cancellationToken) =>
        await _context.LearnedPredictions.ToListAsync(cancellationToken);
}