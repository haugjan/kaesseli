namespace Kaesseli.Domain.Integration;

public interface ITransactionRepository
{
    Task<IEnumerable<TransactionSummary>> GetTransactionSummaries(CancellationToken cancellationToken);
    Task<TransactionSummary> AddTransactionSummary(TransactionSummary transactionSummary, CancellationToken cancellationToken);
}