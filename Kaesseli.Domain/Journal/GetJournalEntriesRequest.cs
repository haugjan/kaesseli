using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Journal;

public class GetJournalEntriesRequest
{
    public required Guid AccountingPeriodId { get; init; }
    public required Guid? AccountId { get; init; }
    public required AccountType? AccountType { get; init; }
}