using Kaesseli.Application.Common;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Journal;

public class AddJournalEntryCommandHandler(IJournalRepository journalRepository,
                                           IDateTimeService dateTime)
    : IRequestHandler<AddJournalEntryCommand, Guid>
{
    public async Task<Guid> Handle(AddJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var valueDate = request.ValueDate ?? dateTime.ToDay;
        var newJournalEntryEntity = new JournalEntry
        {
            Id = Guid.NewGuid(),
            ValueDate = valueDate,
            Amount = request.Amount,
            Description = request.Description,
            CreditAccount = request.CreditAccountId,
            DebitAccount = request.DebitAccountId
        };

        var createdEntry = await journalRepository.AddJournalEntry(newJournalEntryEntity, cancellationToken);
        return createdEntry.Id;
    }
}