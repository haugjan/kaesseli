using Kaesseli.Domain.Prediction;
using MediatR;

namespace Kaesseli.Application.Integration.TransactionAddedEvent;

// ReSharper disable once UnusedType.Global
public class TransactionAddedEventHandler : INotificationHandler<TransactionAddedNotification>
{
    private readonly ITransactionTeachingService _teachingService;

    public TransactionAddedEventHandler(ITransactionTeachingService teachingService) =>
        _teachingService = teachingService;

    public async Task Handle(TransactionAddedNotification notification, CancellationToken cancellationToken)
    {
        var addedEvent = notification.DomainEvent;
        // Aufrufen des TeachingService mit der relevanten Transaktion
        await _teachingService.Teach(addedEvent.TransactionId, addedEvent.AccountId, cancellationToken);
    }
}