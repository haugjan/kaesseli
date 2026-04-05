using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Integration;

public class Transaction
{
    private Transaction() { }

    public Guid Id { get; private init; }
    public string RawText { get; private init; } = null!;
    public decimal Amount { get; private init; }
    public DateOnly ValueDate { get; private init; }
    public string Description { get; private init; } = null!;
    public string Reference { get; private init; } = null!;
    public DateOnly BookDate { get; private init; }
    public string TransactionCode { get; private init; } = null!;
    public string TransactionCodeDetail { get; private init; } = null!;
    public string? Debtor { get; private init; }
    public string? Creditor { get; private init; }
    public TransactionSummary? TransactionSummary { get; private init; }
    public IEnumerable<JournalEntry>? JournalEntries { get; private init; }

    public static Transaction Create(
        string rawText,
        decimal amount,
        DateOnly valueDate,
        string description,
        string reference,
        DateOnly bookDate,
        string transactionCode,
        string transactionCodeDetail,
        string? debtor,
        string? creditor,
        TransactionSummary? transactionSummary = null)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            RawText = rawText,
            Amount = amount,
            ValueDate = valueDate,
            Description = description,
            Reference = reference,
            BookDate = bookDate,
            TransactionCode = transactionCode,
            TransactionCodeDetail = transactionCodeDetail,
            Debtor = debtor,
            Creditor = creditor,
            TransactionSummary = transactionSummary,
            JournalEntries = null,
        };
    }
}
