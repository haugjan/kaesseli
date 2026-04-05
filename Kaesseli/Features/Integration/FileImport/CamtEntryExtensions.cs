using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Features.Integration.FileImport;

internal static class CamtEntryExtensions
{
    extension(FinancialDocument financialDocument)
    {
        public TransactionSummary ToTransactionSummary(Account account) =>
            TransactionSummary.Create(
                account,
                financialDocument.BalanceBefore,
                financialDocument.BalanceAfter,
                financialDocument.ValueDateFrom,
                financialDocument.ValueDateTo,
                financialDocument.Reference,
                financialDocument.Entries.Select(entry => entry.ToTransaction()).ToList());
    }

    extension(FinancialDocumentEntry entry)
    {
        public Transaction ToTransaction() =>
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
                creditor: entry.Creditor);
    }
}
