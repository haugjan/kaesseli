using Kaesseli.Infrastructure.Prediction;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Prediction;

internal static class LearnedPredictionExtensions
{
    internal static LearnedPredictionForMl ToLearnedPredictionForMl(this LearnedPrediction learnedPrediction) =>
        new()
        {
            AccountName = learnedPrediction.AccountName,
            Description = learnedPrediction.Description
        };
}