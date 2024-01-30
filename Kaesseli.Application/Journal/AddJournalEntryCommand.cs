using MediatR;

namespace Kaesseli.Application.Journal;

public class AddJournalEntryCommand : IRequest<Guid>, IRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTimeOffset ValueDate { get; set; }
}