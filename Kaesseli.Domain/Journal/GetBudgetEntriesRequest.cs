namespace Kaesseli.Domain.Journal;

public class GetJournalEntriesRequest
{
    public required Guid? DebitAccountId { get; init; }
    public required Guid? CreditAccountId { get; init; }
    public required DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}