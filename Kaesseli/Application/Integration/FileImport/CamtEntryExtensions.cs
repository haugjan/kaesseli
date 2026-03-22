using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Application.Integration.FileImport;

internal static class CamtEntryExtensions
{
    public static TransactionSummary ToTransactionSummary(this FinancialDocument financialDocument, Account account) =>
        new()
        {
            Id = Guid.NewGuid(),
            Account = account,
            BalanceBefore = financialDocument.BalanceBefore,
            BalanceAfter = financialDocument.BalanceAfter,
            ValueDateFrom = financialDocument.ValueDateFrom,
            ValueDateTo = financialDocument.ValueDateTo,
            Reference = financialDocument.Reference,
            Transactions = financialDocument.Entries.Select(ToTransaction).ToImmutableList()
        };

    public static Transaction ToTransaction(this FinancialDocumentEntry financialDocumentEntry) =>
        new()
        {
            RawText = financialDocumentEntry.RawText,
            Description = financialDocumentEntry.Description,
            Reference = financialDocumentEntry.Reference,
            TransactionCode = financialDocumentEntry.TransactionCode,
            TransactionCodeDetail = financialDocumentEntry.TransactionCodeDetail,
            Amount = financialDocumentEntry.Amount,
            ValueDate = financialDocumentEntry.ValueDate,
            BookDate = financialDocumentEntry.BookDate,
            Id = Guid.NewGuid(),
            TransactionSummary = null,
            Debtor = financialDocumentEntry.Debtor,
            Creditor = financialDocumentEntry.Creditor
        };
}