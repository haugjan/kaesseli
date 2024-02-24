
using System.Collections.Immutable;
using Kaesseli.Application.Integration;
using Kaesseli.Domain.Accounts;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Journal;

internal static class CamtEntryExtensions
{

    public static AccountStatement ToAccountStatement(this CamtDocument camtDocument, Account account) =>
        new()
        {
            Id = Guid.NewGuid(),
            Account = account,
            BalanceBefore = camtDocument.BalanceBefore,
            BalanceAfter = camtDocument.BalanceAfter,
            ValueDateFrom = camtDocument.ValueDateFrom,
            ValueDateTo = camtDocument.ValueDateTo,
            Reference = camtDocument.Reference,
            PaymentEntries = camtDocument.CamtEntries.Select(ToPaymentEntry).ToImmutableList()
        };

    public static PaymentEntry ToPaymentEntry(this CamtEntry camtEntry) =>
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
            Id = Guid.NewGuid()
        };
}