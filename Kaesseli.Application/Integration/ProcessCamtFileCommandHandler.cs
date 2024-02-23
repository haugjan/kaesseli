using System.Diagnostics.CodeAnalysis;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration;

public class ProcessCamtFileCommandHandler : IRequestHandler<ProcessCamtFileCommand, IEnumerable<Guid>>
{
    private ICamtProcessor _camtProcessor;
    private IJournalRepository _journalRepo;
    private readonly IAccountRepository _accountRepo;

    public ProcessCamtFileCommandHandler(ICamtProcessor camtProcessor, IJournalRepository journalRepo, IAccountRepository accountRepo)
    {
        _camtProcessor = camtProcessor;
        _journalRepo = journalRepo;
        _accountRepo = accountRepo;
    }

    public async Task<IEnumerable<Guid>> Handle(ProcessCamtFileCommand request, CancellationToken cancellationToken)
    {
        var entries = await _camtProcessor.ReadCamtFile(request.Content, request.AccountId, cancellationToken);
        var result = new List<Guid>();
        var account = await _accountRepo.GetAccount(request.AccountId, cancellationToken);
        foreach (var camtEntry in entries)
        {
            var preJournalEntry = camtEntry.ToPreJournalEntry( account);
            var newEntry = await _journalRepo.AddPreJournalEntry(preJournalEntry, cancellationToken);
            result.Add(newEntry.Id);
        }
        return result;
    }

}