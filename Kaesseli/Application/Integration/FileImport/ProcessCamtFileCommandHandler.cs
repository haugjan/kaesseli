using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration.FileImport;

public interface IProcessCamtFileCommandHandler
{
    Task<Guid> Handle(ProcessCamtFileCommand request, CancellationToken cancellationToken);
}

public class ProcessCamtFileCommandHandler : IProcessCamtFileCommandHandler
{
    private readonly ICamtProcessor _camtProcessor;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepo;
    private readonly IOpenTransactionAmountChangedEventHandler _eventHandler;

    public ProcessCamtFileCommandHandler(ICamtProcessor camtProcessor, ITransactionRepository transactionRepository, IAccountRepository accountRepo,
                                         IOpenTransactionAmountChangedEventHandler eventHandler)
    {
        _camtProcessor = camtProcessor;
        _transactionRepository = transactionRepository;
        _accountRepo = accountRepo;
        _eventHandler = eventHandler;
    }

    public async Task<Guid> Handle(ProcessCamtFileCommand request, CancellationToken cancellationToken)
    {
        var financialDocument = await _camtProcessor.ReadCamtFile(request.Content, cancellationToken);
        var account = await _accountRepo.GetAccount(request.AccountId, cancellationToken);

        var transactionSummary = financialDocument.ToTransactionSummary(account);
        await _transactionRepository.AddTransactionSummary(transactionSummary, cancellationToken);
        await _eventHandler.Handle(
            notification: new OpenTransactionAmountChangedEvent { Amount = transactionSummary.Transactions.Count() },
            cancellationToken);
        return transactionSummary.Id;
    }
}
