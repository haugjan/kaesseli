namespace Kaesseli.Features.AccountSuggestion;

public class AccountSuggestion
{
    private AccountSuggestion() { }

    public Guid Id { get; private init; }
    public Guid TransactionId { get; private init; }
    public DateTimeOffset GeneratedAt { get; private init; }
    public string? Error { get; private init; }
    public List<AccountSuggestionItem> Items { get; private init; } = [];

    public static AccountSuggestion Create(
        Guid transactionId,
        DateTimeOffset generatedAt,
        IEnumerable<AccountSuggestionItem> items,
        string? error = null
    )
    {
        if (transactionId == Guid.Empty)
            throw new ArgumentException("TransactionId must not be empty.", nameof(transactionId));

        return new AccountSuggestion
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            GeneratedAt = generatedAt,
            Error = error,
            Items = items.ToList(),
        };
    }
}

public class AccountSuggestionItem
{
    private AccountSuggestionItem() { }

    public Guid AccountId { get; private init; }
    public double Confidence { get; private init; }
    public int Rank { get; private init; }

    public static AccountSuggestionItem Create(Guid accountId, double confidence, int rank)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId must not be empty.", nameof(accountId));
        if (confidence is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence), confidence, "Confidence must be between 0 and 1.");
        if (rank < 1)
            throw new ArgumentOutOfRangeException(nameof(rank), rank, "Rank must be >= 1.");

        return new AccountSuggestionItem
        {
            AccountId = accountId,
            Confidence = confidence,
            Rank = rank,
        };
    }
}
