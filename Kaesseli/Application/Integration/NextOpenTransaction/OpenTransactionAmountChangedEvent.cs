using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public class OpenTransactionAmountChangedEvent
{
    public required int Amount { get; init; }
}

public interface IOpenTransactionAmountChangedEventHandler
{
    Task Handle(OpenTransactionAmountChangedEvent notification, CancellationToken cancellationToken);
}

public class OpenTransactionAmountChangedEventHandler : IOpenTransactionAmountChangedEventHandler
{
    private readonly ITransactionRepository _tranRepo;

    public OpenTransactionAmountChangedEventHandler(ITransactionRepository tranRepo)
    {
        _tranRepo = tranRepo;
    }

    public async Task Handle(OpenTransactionAmountChangedEvent notification, CancellationToken cancellationToken)
    {
        await _tranRepo.ChangeTotalOpenTransaction(notification.Amount, cancellationToken);
    }
}
