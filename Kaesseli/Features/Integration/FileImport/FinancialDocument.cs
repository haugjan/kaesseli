namespace Kaesseli.Features.Integration.FileImport;

public class FinancialDocument
{
    public required IEnumerable<FinancialDocumentEntry> Entries { get; init; }
    public required decimal BalanceBefore { get; init; }
    public required decimal BalanceAfter { get; init; }
    public required DateOnly ValueDateFrom { get; init; }
    public required DateOnly ValueDateTo { get; init; }
    public required string Reference { get; init; }
    public bool HasBalanceInfo { get; init; }

    public decimal EntriesTotal => Entries.Sum(entry => entry.Amount);
    public decimal ExpectedDelta => BalanceAfter - BalanceBefore;
    public decimal BalanceDifference => EntriesTotal - ExpectedDelta;
    public bool IsBalanceConsistent => !HasBalanceInfo || BalanceDifference == 0m;
}
