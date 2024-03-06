using Kaesseli.Domain.Integration;

namespace Kaesseli.Domain.Prediction;

public interface ITransactionPredictionService
{
    // ReSharper disable once UnusedMember.Global
    IEnumerable<AccountPrediction> Predict(Transaction transaction);
}