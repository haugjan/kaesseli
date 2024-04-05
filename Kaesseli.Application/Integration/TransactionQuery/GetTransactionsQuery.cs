using MediatR;

namespace Kaesseli.Application.Integration.TransactionQuery;

public class GetTransactionsQuery : IRequest<IEnumerable<GetTransactionsQueryResult>>
{
    public required Guid TransactionSummaryId { get; init; }
}