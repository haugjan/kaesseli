namespace Kaesseli.Application.Integration.TransactionQuery;

public class GetTransactionsQuery
{
    public required Guid TransactionSummaryId { get; init; }
}