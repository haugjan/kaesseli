using Kaesseli.Features.Integration;

namespace Kaesseli.Features.Integration.NextOpenTransaction;

public static class OpenTransactionAmountChanged
{
    public record Event(int Amount);

    public interface IHandler
    {
        Task Handle(Event notification, CancellationToken cancellationToken);
    }

    public class Handler(ITransactionRepository tranRepo) : IHandler
    {
        public async Task Handle(Event notification, CancellationToken cancellationToken)
        {
            await tranRepo.ChangeTotalOpenTransaction(notification.Amount, cancellationToken);
        }
    }
}
