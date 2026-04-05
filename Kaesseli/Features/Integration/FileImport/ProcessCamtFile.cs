using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;

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

            var transactionSummary = TransactionSummary.Create(
                account,
                financialDocument.BalanceBefore,
                financialDocument.BalanceAfter,
                financialDocument.ValueDateFrom,
                financialDocument.ValueDateTo,
                financialDocument.Reference,
                financialDocument.Entries.Select(entry => Transaction.Create(
                    rawText: entry.RawText,
                    amount: entry.Amount,
                    valueDate: entry.ValueDate,
                    description: entry.Description,
                    reference: entry.Reference,
                    bookDate: entry.BookDate,
                    transactionCode: entry.TransactionCode,
                    transactionCodeDetail: entry.TransactionCodeDetail,
                    debtor: entry.Debtor,
                    creditor: entry.Creditor)).ToList());
            await transactionRepository.AddTransactionSummary(transactionSummary, cancellationToken);
            await eventHandler.Handle(
                notification: new OpenTransactionAmountChanged.Event(transactionSummary.Transactions.Count()),
                cancellationToken);
            return transactionSummary.Id;
        }
    }
}
