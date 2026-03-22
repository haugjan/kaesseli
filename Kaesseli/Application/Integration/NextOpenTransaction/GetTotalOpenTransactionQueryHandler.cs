using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class GetTotalOpenTransactionQueryHandler : IRequestHandler<GetTotalOpenTransactionQuery, int>
{
    private readonly ITransactionRepository _transRepo;

    public GetTotalOpenTransactionQueryHandler(ITransactionRepository transRepo)
    {
        _transRepo = transRepo;
    }

    public async Task<int> Handle(GetTotalOpenTransactionQuery request, CancellationToken cancellationToken)
    {
        return await _transRepo.GetTotalOpenTransaction(cancellationToken);
    }
}