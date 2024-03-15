namespace Kaesseli.Domain.Integration;

public interface ITransactionRepository
{
    Task<IEnumerable<TransactionSummary>> GetTransactionSummaries(CancellationToken cancellationToken);
    Task<TransactionSummary> AddTransactionSummary(TransactionSummary transactionSummary, CancellationToken cancellationToken);
    Task<IEnumerable<Transaction>> GetTransactions(Guid transactionSummaryId, CancellationToken cancellationToken);
    Task<Transaction?> GetNextOpenTransaction(int skip, CancellationToken cancellationToken);
    Task<int> GetTotalOpenTransaction(CancellationToken cancellationToken);
    Task<Transaction> GetTransaction(Guid requestTransactionId, CancellationToken cancellationToken);
    Task ChangeTotalOpenTransaction(int notificationAmount, CancellationToken cancellationToken);
}