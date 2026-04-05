using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Journal;

internal static class JournalEntryExtensions
{
    extension(AddJournalEntry.Query request)
    {
        internal JournalEntry ToJournalEntry(
            DateOnly valueDate,
            Account debitAccount,
            Account creditAccount,
            AccountingPeriod accountingPeriod) =>
            JournalEntry.Create(valueDate, request.Description, request.Amount, debitAccount, creditAccount, accountingPeriod);
    }
}
