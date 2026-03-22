using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public interface IGetTotalOpenTransactionQueryHandler
{
    Task<int> Handle(GetTotalOpenTransactionQuery request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class GetTotalOpenTransactionQueryHandler : IGetTotalOpenTransactionQueryHandler
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
