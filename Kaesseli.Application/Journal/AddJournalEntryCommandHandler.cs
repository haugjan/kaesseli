using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Journal;

public class AddJournalEntryCommandHandler(IJournalRepository journalRepository,
                                           IAccountRepository accountRepo,
                                           IDateTimeService dateTime)
    : IRequestHandler<AddJournalEntryCommand, Guid>
{
    public async Task<Guid> Handle(AddJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var valueDate = request.ValueDate ?? dateTime.ToDay;
        var creditAccount = await accountRepo.GetAccount(request.CreditAccountId, cancellationToken);
        var debitAccount = await accountRepo.GetAccount(request.DebitAccountId, cancellationToken);

        var newJournalEntryEntity = request.ToJournalEntry(valueDate, debitAccount, creditAccount);

        var createdEntry = await journalRepository.AddJournalEntry(newJournalEntryEntity, cancellationToken);
        return createdEntry.Id;
    }
}