namespace Kaesseli.Domain.Journal;

public class GetJournalEntriesRequest
{
    public Guid? AccountId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}