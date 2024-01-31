using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Journal;

public class AssignAccountToJournalEntryCommandHandler(IJournalRepository journalRepository)
    : IRequestHandler<AssignAccountToJournalEntryCommand>
{
    public async Task Handle(AssignAccountToJournalEntryCommand request, CancellationToken cancellationToken)
        => await journalRepository.AssignAccount(request.JournalEntryId, request.AccountId);
}