namespace Kaesseli.Application.Integration;

public class CamtDocument
{
    public required IEnumerable<CamtEntry> CamtEntries { get; init; }
    public required decimal BalanceBefore { get; init; }
    public required decimal BalanceAfter { get; init; }
    public required DateOnly ValueDateFrom { get; init; }
    public required DateOnly ValueDateTo { get; init; }
    public required string Reference { get; init; }
}