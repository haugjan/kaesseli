using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;

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
        UpdateOpenTransactionTotal.IHandler updateOpenTotal
    ) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var financialDocument = await camtProcessor.ReadCamtFile(
                request.Content,
                cancellationToken
            );
            var account = await accountRepo.GetAccount(request.AccountId, cancellationToken);
            var existingReferences = await transactionRepository.GetExistingTransactionReferences(
                cancellationToken
            );

            var newTransactions = financialDocument
                .Entries.Where(entry => !existingReferences.Contains(entry.Reference))
                .Select(entry =>
                    Transaction.Create(
                        rawText: entry.RawText,
                        amount: entry.Amount,
                        valueDate: entry.ValueDate,
                        description: entry.Description,
                        reference: entry.Reference,
                        bookDate: entry.BookDate,
                        transactionCode: entry.TransactionCode,
                        transactionCodeDetail: entry.TransactionCodeDetail,
                        debtor: entry.Debtor,
                        creditor: entry.Creditor
                    )
                )
                .ToList();

            if (newTransactions.Count == 0)
                return Guid.Empty;

            var transactionSummary = TransactionSummary.Create(
                account,
                financialDocument.BalanceBefore,
                financialDocument.BalanceAfter,
                financialDocument.ValueDateFrom,
                financialDocument.ValueDateTo,
                financialDocument.Reference,
                newTransactions
            );
            await transactionRepository.AddTransactionSummary(
                transactionSummary,
                cancellationToken
            );
            await updateOpenTotal.Handle(
                new UpdateOpenTransactionTotal.Query(newTransactions.Count),
                cancellationToken
            );
            return transactionSummary.Id;
        }
    }
}
