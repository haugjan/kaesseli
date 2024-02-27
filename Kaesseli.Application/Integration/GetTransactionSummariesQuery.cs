using MediatR;

namespace Kaesseli.Application.Integration;

public class GetTransactionSummariesQuery : IRequest<IEnumerable<GetTransactionSummariesQueryResult>>;