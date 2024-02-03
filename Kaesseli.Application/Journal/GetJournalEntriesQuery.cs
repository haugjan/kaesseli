using MediatR;

namespace Kaesseli.Application.Journal;

public class GetJournalEntriesQuery : IRequest<IEnumerable<GetJournalEntriesQueryResult>>
{
    public required Guid? AccountId { get; init; }
    public required DateOnly? FromDate { get; init; }
    public required DateOnly? ToDate { get; init; }
}