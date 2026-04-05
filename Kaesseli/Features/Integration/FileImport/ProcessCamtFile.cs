using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;

namespace Kaesseli.Features.Integration.FileImport;

public static class ProcessCamtFile
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(Stream Content, Guid AccountId);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(
        ICamtProcessor camtProcessor,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepo,
        OpenTransactionAmountChanged.IHandler eventHandler) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var financialDocument = await camtProcessor.ReadCamtFile(request.Content, cancellationToken);
            var account = await accountRepo.GetAccount(request.AccountId, cancellationToken);

            var transactionSummary = financialDocument.ToTransactionSummary(account);
            await transactionRepository.AddTransactionSummary(transactionSummary, cancellationToken);
            await eventHandler.Handle(
                notification: new OpenTransactionAmountChanged.Event(transactionSummary.Transactions.Count()),
                cancellationToken);
            return transactionSummary.Id;
        }
    }
}
