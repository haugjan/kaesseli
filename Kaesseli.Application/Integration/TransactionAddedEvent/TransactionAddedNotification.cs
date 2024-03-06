using MediatR;

namespace Kaesseli.Application.Integration.TransactionAddedEvent;

public class TransactionAddedNotification : INotification
{
    public TransactionAddedNotification(Domain.Integration.TransactionAddedEvent domainEvent) =>
        DomainEvent = domainEvent;

    public Domain.Integration.TransactionAddedEvent DomainEvent { get; }
}