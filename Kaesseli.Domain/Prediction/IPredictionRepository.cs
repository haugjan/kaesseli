namespace Kaesseli.Domain.Prediction;

public interface IPredictionRepository
{
    Task AddLearnedPrediction(Guid transactionId, Guid accountId, CancellationToken cancellationToken);
    Task<IEnumerable<LearnedPrediction>> GetAllLearnedPredictions(CancellationToken cancellationToken);
}