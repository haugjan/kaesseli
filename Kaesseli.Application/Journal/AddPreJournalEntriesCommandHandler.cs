//using System.Collections.Immutable;
//using Kaesseli.Domain.Accounts;
//using Kaesseli.Domain.Journal;
//using MediatR;

//namespace Kaesseli.Application.Journal;

//public class AddPreJournalEntriesCommandHandler : IRequestHandler<AddPreJournalEntriesCommand, IEnumerable<Guid>>
//{
//    private readonly IJournalRepository _journalRepo;
//    private readonly IAccountRepository _accountRepo;

//    public AddPreJournalEntriesCommandHandler(IJournalRepository journalRepo, IAccountRepository accountRepo)
//    {
//        _journalRepo = journalRepo;
//        _accountRepo = accountRepo;
//    }

//    public async Task<IEnumerable<Guid>> Handle(AddPreJournalEntriesCommand request, CancellationToken cancellationToken)
//    {
//        var result = new List<Guid>();
//        foreach (var entry in request.Entries)
//        {
//            var account = await _accountRepo.GetAccount(entry.AccountId, cancellationToken);

//            var newAddedEntry = await _journalRepo.AddPreJournalEntry(
//                                    entry: new PreJournalEntry
//                                    {
//                                        RawText = entry.RawText,
//                                        Account = account,
//                                        Amount = entry.Amount,
//                                        ValueDate = entry.ValueDate,
//                                        Id = Guid.NewGuid()
//                                    },
//                                    cancellationToken);
//            result.Add(newAddedEntry.Id);
//        }

//        return result.ToImmutableList();
//    }
//}