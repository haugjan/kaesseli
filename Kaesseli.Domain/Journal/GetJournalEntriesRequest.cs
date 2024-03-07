using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Journal;

public class GetJournalEntriesRequest
{
    public required Guid AccountingPeriodId { get; init; }
    public Guid? DebitAccountId { get; init; }
    public Guid? CreditAccountId { get; init; }
    public Guid? AccountId { get; init; }
    public AccountType? AccountType { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}