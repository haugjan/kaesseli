using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public static class GetTotalOpenTransaction
{
    public record Query;

    public interface IHandler
    {
        Task<int> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository transRepo) : IHandler
    {
        public async Task<int> Handle(Query request, CancellationToken cancellationToken)
        {
            return await transRepo.GetTotalOpenTransaction(cancellationToken);
        }
    }
}
