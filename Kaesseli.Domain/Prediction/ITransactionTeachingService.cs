namespace Kaesseli.Domain.Prediction;

public interface ITransactionTeachingService
{
    Task Teach(Guid transactionId, Guid accountId, CancellationToken cancellationToken);

}