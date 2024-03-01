using System.Collections.Immutable;
using Kaesseli.Application.Integration.Camt;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Journal;

internal static class CamtEntryExtensions
{
    public static TransactionSummary ToTransactionSummary(this CamtDocument camtDocument, Account account) =>
        new()
        {
            Id = Guid.NewGuid(),
            Account = account,
            BalanceBefore = camtDocument.BalanceBefore,
            BalanceAfter = camtDocument.BalanceAfter,
            ValueDateFrom = camtDocument.ValueDateFrom,
            ValueDateTo = camtDocument.ValueDateTo,
            Reference = camtDocument.Reference,
            Transactions = camtDocument.CamtEntries.Select(ToTransaction).ToImmutableList()
        };

    public static Transaction ToTransaction(this CamtEntry camtEntry) =>
        new()
        {
            RawText = camtEntry.RawText,
            Description = camtEntry.Description,
            Reference = camtEntry.Reference,
            TransactionCode = camtEntry.TransactionCode,
            TransactionCodeDetail = camtEntry.TransactionCodeDetail,
            Amount = camtEntry.Amount,
            ValueDate = camtEntry.ValueDate,
            BookDate = camtEntry.BookDate,
            Id = Guid.NewGuid(),
            TransactionSummary = null
        };
}