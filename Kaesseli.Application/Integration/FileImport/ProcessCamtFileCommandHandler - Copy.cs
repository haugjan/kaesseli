using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Integration.FileImport;

public class ProcessPostFinanceCsvCommandHandler : IRequestHandler<ProcessPostFinanceCsvCommand, Guid>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepo;
    private readonly IMediator _mediator;
    private readonly IPostFinanceCsvProcessor _postFinanceProcessor;

    public ProcessPostFinanceCsvCommandHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepo,
                                               IMediator mediator,
                                               IPostFinanceCsvProcessor postFinanceProcessor)
    {
        _transactionRepository = transactionRepository;
        _accountRepo = accountRepo;
        _mediator = mediator;
        _postFinanceProcessor = postFinanceProcessor;
    }

    public async Task<Guid> Handle(ProcessPostFinanceCsvCommand request, CancellationToken cancellationToken)
    {
        var financialDocument = await _postFinanceProcessor.ReadCsvFile(request.Content, cancellationToken);
        var account = await _accountRepo.GetAccount(request.AccountId, cancellationToken);

        var  transactionSummary = financialDocument.ToTransactionSummary(account);
        await _transactionRepository.AddTransactionSummary(transactionSummary, cancellationToken);
        await _mediator.Publish(
            notification: new OpenTransactionAmountChangedEvent { Amount = transactionSummary.Transactions.Count() },
            cancellationToken);
        return transactionSummary.Id;
    }

}