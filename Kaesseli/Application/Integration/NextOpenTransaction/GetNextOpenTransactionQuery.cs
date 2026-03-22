using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public class GetNextOpenTransactionQuery : IRequest<GetNextOpenTransactionQueryResult>
{
    public required int Skip { get; init; }
}