using Kaesseli.Domain.Prediction;
using Microsoft.ML;

namespace Kaesseli.Infrastructure.Prediction;

public class TransactionTeachingService : ITransactionTeachingService
{
    private readonly IPredictionRepository _predictionRepo;
    private readonly MLContext _mlContext;

    public TransactionTeachingService(IPredictionRepository predictionRepo)
    {
        _predictionRepo = predictionRepo;
        _mlContext = new MLContext(seed: 0);
    }

    public async Task Teach(Guid transactionId, Guid accountId, CancellationToken cancellationToken)
    {
        await SaveTransactionData(transactionId, accountId, cancellationToken);

        await RetrainModel(cancellationToken);
    }

    private async Task SaveTransactionData(Guid transactionId, Guid accountId, CancellationToken cancellationToken) =>
        await _predictionRepo.AddLearnedPrediction(transactionId, accountId, cancellationToken);

    private async Task RetrainModel(CancellationToken cancellationToken)
    {
        var trainingData = await LoadTrainingData(cancellationToken);
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Definieren der Datenverarbeitungspipeline
        var dataProcessPipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                                                outputColumnName: "Label",
                                                inputColumnName: nameof(LearnedPredictionForMl.AccountName))
                                            .Append(
                                                estimator: _mlContext.Transforms.Text.FeaturizeText(
                                                    outputColumnName: "Features",
                                                    inputColumnName: nameof(LearnedPredictionForMl.Description)))
                                            .AppendCacheCheckpoint(_mlContext);

        // Auswahl des Algorithmus für das maschinelle Lernen
        var trainer = _mlContext.MulticlassClassification.Trainers
                                .SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features")
                                .Append(estimator: _mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel"));

        // Erstellen und Trainieren des Modells
        var trainingPipeline = dataProcessPipeline.Append(trainer);
        var trainedModel = trainingPipeline.Fit(dataView);

        // Speichern des trainierten Modells
        SaveModel(trainedModel);
    }

    private async Task<IEnumerable<LearnedPredictionForMl>> LoadTrainingData(CancellationToken cancellationToken)
    {
        var predictions = await _predictionRepo.GetAllLearnedPredictions(cancellationToken);
        return predictions.Select(prediction => prediction.ToLearnedPredictionForMl());
    }

    private void SaveModel(ITransformer model) =>
        _mlContext.Model.Save(model, inputSchema: null, filePath: "model.zip");
}