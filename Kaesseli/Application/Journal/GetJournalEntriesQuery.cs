using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Journal;

public class GetJournalEntriesQuery
{
    public required Guid AccountingPeriodId { get; init; }
    public required Guid? AccountId { get; init; }
    public required AccountType? AccountType { get; init; }
}