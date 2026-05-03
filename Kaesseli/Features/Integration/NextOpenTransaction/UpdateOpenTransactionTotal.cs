using Kaesseli.Features.Integration;

namespace Kaesseli.Features.Integration.NextOpenTransaction;

public static class UpdateOpenTransactionTotal
{
    public record Query(int Delta);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(ITransactionRepository tranRepo) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            await tranRepo.ChangeTotalOpenTransaction(request.Delta, cancellationToken);
        }
    }
}
