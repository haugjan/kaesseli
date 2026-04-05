namespace Kaesseli.Contracts.Integration;

public record Transaction(
    Guid Id,
    string RawText,
    decimal Amount,
    DateOnly ValueDate,
    DateOnly BookDate,
    string Description,
    string Reference,
    string TransactionCode,
    string TransactionCodeDetail,
    string? Debtor,
    string? Creditor);
