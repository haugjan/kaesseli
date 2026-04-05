namespace Kaesseli.Contracts.Features.Integration.TransactionQuery;

public static class GetTransactionsContract
{
    public record Result(
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
}
