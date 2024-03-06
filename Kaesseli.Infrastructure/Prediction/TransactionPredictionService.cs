using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Prediction;
using Microsoft.ML;

namespace Kaesseli.Infrastructure.Prediction;

public class TransactionPredictionService : ITransactionPredictionService
{
    private readonly MLContext _mlContext = new(seed: 0);

    public IEnumerable<AccountPrediction> Predict(Transaction transaction)
    {
        // ReSharper disable once UnusedVariable
        var model = _mlContext.Model.Load(filePath: "model.zip", inputSchema: out var schema);

        // Erstellen eines PredictionEngine
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<LearnedPredictionForMl, AccountPrediction>(model);

        // Erstellen der Eingabedaten
        var input = new LearnedPredictionForMl { AccountName = null!, Description = transaction.Description };

        // Vorhersage durchführen
        var prediction = predictionEngine.Predict(input);

        return new List<AccountPrediction>
        {
            new() { Probability = prediction.Probability, Account = prediction.Account }
        };

        //// Umwandlung der Vorhersage in eine Liste von AccountName mit Wahrscheinlichkeiten
        //var results = new List<(string AccountName, float Probability)>();
        //for (var i = 0; i < prediction.Score.Length; i++)
        //{
        //    var accountName =schema.GetColumnOrNull("Label");
        //    var probability = prediction. .Score[i];
        //    results.Add(item: (accountName, probability));
        //}

        //return results.Select(r=> new AccountPrediction()
        //{
        //    Score = r.Probability
        //})
    }
}

