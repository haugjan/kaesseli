namespace Kaesseli.Application.Integration.FileImport;

public class FinancialDocument
{
    public required IEnumerable<FinancialDocumentEntry> Entries { get; init; }
    public required decimal BalanceBefore { get; init; }
    public required decimal BalanceAfter { get; init; }
    public required DateOnly ValueDateFrom { get; init; }
    public required DateOnly ValueDateTo { get; init; }
    public required string Reference { get; init; }
}