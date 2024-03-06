using Kaesseli.Domain.Journal;

namespace Kaesseli.Domain.Integration;

public class Transaction
{
    public required string RawText { get; init; }
    public required decimal Amount { get; init; }
    public required DateOnly ValueDate { get; init; }
    public required Guid Id { get; init; }
    public required string Description { get; init; }
    public required string Reference { get; init; }
    public required DateOnly BookDate { get; init; }
    public required string TransactionCode { get; init; }
    public required string TransactionCodeDetail { get; init; }
    public required string? Debtor { get; init; }
    public required string? Creditor { get; init; }
    public required TransactionSummary? TransactionSummary { get; init; }
    public IEnumerable<JournalEntry>? JournalEntries { get; init; }
}