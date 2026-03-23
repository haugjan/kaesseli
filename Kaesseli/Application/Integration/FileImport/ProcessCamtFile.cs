using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration.FileImport;

public static class ProcessCamtFile
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Stream Content { get; init; }
        public required Guid AccountId { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        private readonly ICamtProcessor _camtProcessor;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepo;
        private readonly OpenTransactionAmountChanged.IHandler _eventHandler;

        public Handler(ICamtProcessor camtProcessor, ITransactionRepository transactionRepository, IAccountRepository accountRepo,
                       OpenTransactionAmountChanged.IHandler eventHandler)
        {
            _camtProcessor = camtProcessor;
            _transactionRepository = transactionRepository;
            _accountRepo = accountRepo;
            _eventHandler = eventHandler;
        }

        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var financialDocument = await _camtProcessor.ReadCamtFile(request.Content, cancellationToken);
            var account = await _accountRepo.GetAccount(request.AccountId, cancellationToken);

            var transactionSummary = financialDocument.ToTransactionSummary(account);
            await _transactionRepository.AddTransactionSummary(transactionSummary, cancellationToken);
            await _eventHandler.Handle(
                notification: new OpenTransactionAmountChanged.Event { Amount = transactionSummary.Transactions.Count() },
                cancellationToken);
            return transactionSummary.Id;
        }
    }
}
