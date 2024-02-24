using System.Diagnostics.CodeAnalysis;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration;

public class ProcessCamtFileCommandHandler : IRequestHandler<ProcessCamtFileCommand, Guid>
{
    private readonly ICamtProcessor _camtProcessor;
    private readonly IJournalRepository _journalRepo;
    private readonly IAccountRepository _accountRepo;

    public ProcessCamtFileCommandHandler(ICamtProcessor camtProcessor, IJournalRepository journalRepo, IAccountRepository accountRepo)
    {
        _camtProcessor = camtProcessor;
        _journalRepo = journalRepo;
        _accountRepo = accountRepo;
    }

    public async Task<Guid> Handle(ProcessCamtFileCommand request, CancellationToken cancellationToken)
    {
        var camtDocument = await _camtProcessor.ReadCamtFile(request.Content, request.AccountId, cancellationToken);
        var account = await _accountRepo.GetAccount(request.AccountId, cancellationToken);

        var accountStatement = camtDocument.ToAccountStatement(account);
        await _journalRepo.AddAccountStatement(accountStatement, cancellationToken);
        return accountStatement.Id;
    }

}