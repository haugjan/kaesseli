using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Integration.Camt;

public class ProcessCamtFileCommandHandler : IRequestHandler<ProcessCamtFileCommand, Guid>
{
    private readonly ICamtProcessor _camtProcessor;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepo;

    public ProcessCamtFileCommandHandler(ICamtProcessor camtProcessor, ITransactionRepository transactionRepository, IAccountRepository accountRepo)
    {
        _camtProcessor = camtProcessor;
        _transactionRepository = transactionRepository;
        _accountRepo = accountRepo;
    }

    public async Task<Guid> Handle(ProcessCamtFileCommand request, CancellationToken cancellationToken)
    {
        var camtDocument = await _camtProcessor.ReadCamtFile(request.Content, cancellationToken);
        var account = await _accountRepo.GetAccount(request.AccountId, cancellationToken);

        var  transactionSummary = camtDocument.ToTransactionSummary(account);
        await _transactionRepository.AddTransactionSummary(transactionSummary, cancellationToken);

        return transactionSummary.Id;
    }

}