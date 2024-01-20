using Kässeli.Domain.Repositories;
using MediatR;

namespace Kässeli.Application.Journal;

public class AssignAccountToJournalEntryCommandHandler(IJournalRepository journalRepository)
    : IRequestHandler<AssignAccountToJournalEntryCommand>
{
    public async Task Handle(AssignAccountToJournalEntryCommand request, CancellationToken cancellationToken)
        => await journalRepository.AssignAccount(request.JournalEntryId, request.AccountId);
}