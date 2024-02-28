using MediatR;

namespace Kaesseli.Application.Integration;

public class GetTransactionsQuery : IRequest<IEnumerable<GetTransactionsQueryResult>>
{
    public required Guid TransactionSummaryId { get; init; }
}