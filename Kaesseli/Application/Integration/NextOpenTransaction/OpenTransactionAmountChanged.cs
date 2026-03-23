using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public static class OpenTransactionAmountChanged
{
    public class Event
    {
        public required int Amount { get; init; }
    }

    public interface IHandler
    {
        Task Handle(Event notification, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        private readonly ITransactionRepository _tranRepo;

        public Handler(ITransactionRepository tranRepo)
        {
            _tranRepo = tranRepo;
        }

        public async Task Handle(Event notification, CancellationToken cancellationToken)
        {
            await _tranRepo.ChangeTotalOpenTransaction(notification.Amount, cancellationToken);
        }
    }
}
