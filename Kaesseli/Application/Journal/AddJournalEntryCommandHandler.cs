using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Journal;

public interface IAddJournalEntryCommandHandler
{
    Task<Guid> Handle(AddJournalEntryCommand request, CancellationToken cancellationToken);
}

public class AddJournalEntryCommandHandler(IJournalRepository journalRepository,
                                           IAccountRepository accountRepo,
                                           IDateTimeService dateTime)
    : IAddJournalEntryCommandHandler
{
    public async Task<Guid> Handle(AddJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var valueDate = request.ValueDate ?? dateTime.ToDay;
        var creditAccount = await accountRepo.GetAccount(request.CreditAccountId, cancellationToken);
        var debitAccount = await accountRepo.GetAccount(request.DebitAccountId, cancellationToken);
        var accountingPeriod = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);

        var newJournalEntryEntity = request.ToJournalEntry(valueDate, debitAccount, creditAccount, accountingPeriod);

        var createdEntry = await journalRepository.AddJournalEntry(newJournalEntryEntity, cancellationToken);
        return createdEntry.Id;
    }
}
