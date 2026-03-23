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
    public class Handler : IHandler
    {
        private readonly ITransactionRepository _transRepo;

        public Handler(ITransactionRepository transRepo)
        {
            _transRepo = transRepo;
        }

        public async Task<int> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _transRepo.GetTotalOpenTransaction(cancellationToken);
        }
    }
}
