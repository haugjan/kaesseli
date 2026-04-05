namespace Kaesseli.Contracts.Integration;

public record TransactionSummary(
    Guid Id,
    string AccountName,
    DateOnly ValueDateFrom,
    DateOnly ValueDateTo,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string Reference,
    int NrOfTransactions);
