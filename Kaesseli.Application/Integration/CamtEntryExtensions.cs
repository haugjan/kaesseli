
using Kaesseli.Application.Integration;
using Kaesseli.Domain.Accounts;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Journal;

internal static class CamtEntryExtensions
{
    public static PreJournalEntry ToPreJournalEntry(this CamtEntry camtEntry, Account account) =>
        new()
        {
            RawText = camtEntry.RawText,
            Description = camtEntry.Description,
            Reference = camtEntry.Reference,
            TransactionCode = camtEntry.TransactionCode,
            TransactionCodeDetail = camtEntry.TransactionCodeDetail,
            Account = account,
            Amount = camtEntry.Amount,
            ValueDate = camtEntry.ValueDate,
            BookDate = camtEntry.BookDate,
            Id = Guid.NewGuid()
        };
}