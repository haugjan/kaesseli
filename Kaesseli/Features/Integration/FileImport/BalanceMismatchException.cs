namespace Kaesseli.Features.Integration.FileImport;

public class BalanceMismatchException(FinancialDocument document) : Exception(
    $"Balance mismatch: expected delta {document.ExpectedDelta:N2}, " +
    $"actual entries sum {document.EntriesTotal:N2}, " +
    $"difference {document.BalanceDifference:N2}.")
{
    public FinancialDocument Document { get; } = document;
}
