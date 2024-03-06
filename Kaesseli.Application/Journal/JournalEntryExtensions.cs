using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Journal;

internal static class JournalEntryExtensions
{
    internal static JournalEntry ToJournalEntry(
        this AddJournalEntryCommand request,
        DateOnly valueDate,
        Account debitAccount,
        Account creditAccount,
        AccountingPeriod accountingPeriod) =>
        new()
        {
            Id = Guid.NewGuid(),
            ValueDate = valueDate,
            Amount = request.Amount,
            Description = request.Description,
            CreditAccount = creditAccount,
            DebitAccount = debitAccount,
            Transaction = null,
            AccountingPeriod = accountingPeriod
        };
}