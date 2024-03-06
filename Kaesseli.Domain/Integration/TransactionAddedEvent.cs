namespace Kaesseli.Domain.Integration;

public class TransactionAddedEvent
{
    public required Guid TransactionId { get; init; }
    public required Guid AccountId { get; init; }
}