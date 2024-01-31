using MediatR;

namespace Kaesseli.Application.Journal;

public class AddJournalEntryCommand : IRequest<Guid>
{
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset ValueDate { get; init; }
}