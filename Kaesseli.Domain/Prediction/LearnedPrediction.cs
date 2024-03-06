namespace Kaesseli.Domain.Prediction;

public class LearnedPrediction
{
    public required Guid Id { get; init; }
    public required string AccountName { get; init; }
    public required string Description{ get; init; }
    public required decimal Amount{ get; init; }
    public required DateOnly BookDate{ get; init; }
    public required DateOnly ValueDate { get; init; }
    public required string? Debtor { get; init; }
    public required string? Creditor { get; init; }
}