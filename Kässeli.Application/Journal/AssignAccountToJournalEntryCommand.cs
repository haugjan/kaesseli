using MediatR;

namespace Kässeli.Application.Journal;

public class AssignAccountToJournalEntryCommand : IRequest
{
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
}