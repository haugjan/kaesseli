namespace Kaesseli.Domain.Journal;

public class AssignOpenTransactionEntry
{
    public required Guid OtherAccountId { get; init; }
    public required decimal Amount { get; init; }
}