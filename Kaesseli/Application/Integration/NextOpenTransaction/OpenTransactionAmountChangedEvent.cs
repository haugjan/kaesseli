using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public class OpenTransactionAmountChangedEvent : INotification
{
    public required int Amount { get; init; }
}

public class OpenTransactionAmountChangedEventHandler : INotificationHandler<OpenTransactionAmountChangedEvent>
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