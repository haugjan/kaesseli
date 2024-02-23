namespace Kaesseli.Application.Integration;

public class CamtEntry
{
    public required string Description { get; init; }
    public required string RawText { get; init; }
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    public required DateOnly ValueDate { get; init; }
    public required DateOnly BookDate { get; init; }
    public required string Reference { get; init; }
    public required string TransactionCode { get; init; }
    public required string TransactionCodeDetail { get; init; }
}