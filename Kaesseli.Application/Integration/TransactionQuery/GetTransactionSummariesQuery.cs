using MediatR;

namespace Kaesseli.Application.Integration.TransactionQuery;

public class GetTransactionSummariesQuery : IRequest<IEnumerable<GetTransactionSummariesQueryResult>>;