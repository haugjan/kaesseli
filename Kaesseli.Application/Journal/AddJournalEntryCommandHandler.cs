using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Journal;

public class AddJournalEntryCommandHandler(IJournalRepository journalRepository)
    : IRequestHandler<AddJournalEntryCommand, Guid>
{
    public async Task<Guid> Handle(AddJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var newJournalEntryEntity = new JournalEntry()
        {
            Id = Guid.NewGuid(),
            ValueDate = request.ValueDate,
            Amount = request.Amount,
            Description = request.Description,
        };

        var createdEntry = await journalRepository.AddJournalEntry(newJournalEntryEntity, cancellationToken);
        return createdEntry.Id;
    }
}