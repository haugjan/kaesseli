using Kaesseli.Application.Integration;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Integration;

public static class TransactionSummaryExtensions
{
    public static GetTransactionSummariesQueryResult ToGetTransactionSummary(this TransactionSummary transactionSummary) =>
        new()
        {
            Id = transactionSummary.Id,
            AccountName = transactionSummary.Account.Name,
            ValueDateFrom = transactionSummary.ValueDateFrom,
            ValueDateTo = transactionSummary.ValueDateTo,
            BalanceBefore = transactionSummary.BalanceBefore,
            BalanceAfter = transactionSummary.BalanceAfter,
            Reference = transactionSummary.Reference,
            NrOfTransactions = transactionSummary.Transactions.Count()
        };
}