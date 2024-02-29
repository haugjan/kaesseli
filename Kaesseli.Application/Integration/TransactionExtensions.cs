using Kaesseli.Domain.Integration;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Application.Integration;

public static class TransactionExtensions
{
    public static GetTransactionsQueryResult ToGetTransactionSummary(this Transaction transactionSummary) =>
        new()
        {
            Id = transactionSummary.Id,
            RawText = transactionSummary.RawText,
            Amount = transactionSummary.Amount,
            ValueDate = transactionSummary.ValueDate,
            BookDate = transactionSummary.BookDate,
            Description = transactionSummary.Description,
            Reference = transactionSummary.Reference,
            TransactionCode = transactionSummary.TransactionCode,
            TransactionCodeDetail = transactionSummary.TransactionCodeDetail
        };
}