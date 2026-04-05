using Kaesseli.Features.Integration;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Features.Integration.TransactionQuery;

public static class TransactionSummaryExtensions
{
    extension(TransactionSummary transactionSummary)
    {
        public GetTransactionSummaries.Result ToGetTransactionSummary() =>
            new(
                Id: transactionSummary.Id,
                AccountName: transactionSummary.Account.Name,
                ValueDateFrom: transactionSummary.ValueDateFrom,
                ValueDateTo: transactionSummary.ValueDateTo,
                BalanceBefore: transactionSummary.BalanceBefore,
                BalanceAfter: transactionSummary.BalanceAfter,
                Reference: transactionSummary.Reference,
                NrOfTransactions: transactionSummary.Transactions.Count());
    }
}
