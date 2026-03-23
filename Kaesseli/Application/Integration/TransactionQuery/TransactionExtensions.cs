using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Domain.Accounts;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Domain.Integration;

public static class TransactionExtensions
{
    public static GetTransactions.Result ToGetTransactionSummary(this Transaction transactionSummary) =>
        new()
        {
            Id = transactionSummary.Id,
            RawText = transactionSummary.RawText,
            Amount = transactionSummary.Amount,
            ValueDate = transactionSummary.ValueDate,
            BookDate = transactionSummary.BookDate,
            Description = transactionSummary.Description,
            Reference = transactionSummary.Reference,
            TransactionCode = transactionSummary.TransactionCode,
            TransactionCodeDetail = transactionSummary.TransactionCodeDetail,
            Debtor = transactionSummary.Debtor,
            Creditor = transactionSummary.Creditor
        };

    public static GetNextOpenTransaction.Result ToGetNextOpenTransactionResult(
        this Transaction transaction,
        IEnumerable<Account> accounts) =>
        new()
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            ValueDate = transaction.ValueDate,
            Description = transaction.Description,
            SuggestedAccounts = accounts.Select(account => new GetNextOpenTransaction.SuggestedAccount
            {
                Relevance = 0,
                AccountId = account.Id,
                AccountName = account.Name,
                AccountType = account.Type.DisplayName(),
                AccountTypeId = account.Type,
                AccountIcon = account.Icon.Name,
                AccountIconColor = account.Icon.Color
            }),
            AccountName = transaction.TransactionSummary!.Account.Name,
            AccountType = transaction.TransactionSummary!.Account.Type.DisplayName(),
            AccountTypeId = transaction.TransactionSummary!.Account.Type
        };
}
