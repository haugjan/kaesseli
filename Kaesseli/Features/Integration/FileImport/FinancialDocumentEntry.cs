namespace Kaesseli.Features.Integration.FileImport;

public class FinancialDocumentEntry
{
    public required string Description { get; init; }
    public required string RawText { get; init; }
    public required decimal Amount { get; init; }
    public required DateOnly ValueDate { get; init; }
    public required DateOnly BookDate { get; init; }
    public required string Reference { get; init; }
    public required string TransactionCode { get; init; }
    public required string TransactionCodeDetail { get; init; }
    public required string? Debtor { get; init; }
    public required string? Creditor { get; init; }
}