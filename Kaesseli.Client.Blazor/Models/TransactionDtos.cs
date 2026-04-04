namespace Kaesseli.Client.Blazor.Models;

public record TransactionSummaryDto(
    Guid Id,
    string AccountName,
    DateOnly ValueDateFrom,
    DateOnly ValueDateTo,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string Reference,
    int NrOfTransactions);

public record TransactionDto(
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
