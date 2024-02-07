namespace Kaesseli.Domain.Journal;

public class GetJournalEntriesRequest
{
    public required Guid? AccountId { get; init; }
    public required DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}