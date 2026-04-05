namespace Kaesseli.Contracts.Features.Integration.TransactionQuery;

public static class GetTransactionSummariesContract
{
    public record Result(
        Guid Id,
        string AccountName,
        DateOnly ValueDateFrom,
        DateOnly ValueDateTo,
        decimal BalanceBefore,
        decimal BalanceAfter,
        string Reference,
        int NrOfTransactions);
}
