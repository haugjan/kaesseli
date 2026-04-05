using Kaesseli.Features.Integration;

namespace Kaesseli.Features.Integration.NextOpenTransaction;

public static class GetTotalOpenTransaction
{
    public interface IHandler
    {
        Task<int> Handle(CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository transRepo) : IHandler
    {
        public async Task<int> Handle(CancellationToken cancellationToken)
        {
            return await transRepo.GetTotalOpenTransaction(cancellationToken);
        }
    }
}
