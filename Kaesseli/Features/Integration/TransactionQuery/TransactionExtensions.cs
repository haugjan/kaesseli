using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Integration.TransactionQuery;
using Kaesseli.Features.Accounts;

// ReSharper disable once CheckNamespace
namespace Kaesseli.Features.Integration;

public static class TransactionExtensions
{
    extension(Transaction transaction)
    {
        public GetTransactions.Result ToGetTransactionSummary() =>
            new(
                Id: transaction.Id,
                RawText: transaction.RawText,
                Amount: transaction.Amount,
                ValueDate: transaction.ValueDate,
                BookDate: transaction.BookDate,
                Description: transaction.Description,
                Reference: transaction.Reference,
                TransactionCode: transaction.TransactionCode,
                TransactionCodeDetail: transaction.TransactionCodeDetail,
                Debtor: transaction.Debtor,
                Creditor: transaction.Creditor);

        public GetNextOpenTransaction.Result ToGetNextOpenTransactionResult(
            IEnumerable<Account> accounts) =>
            new(
                Id: transaction.Id,
                Amount: transaction.Amount,
                ValueDate: transaction.ValueDate,
                Description: transaction.Description,
                SuggestedAccounts: accounts.Select(account => new GetNextOpenTransaction.SuggestedAccount(
                    Relevance: 0,
                    AccountId: account.Id,
                    AccountName: account.Name,
                    AccountType: account.Type.DisplayName(),
                    AccountTypeId: account.Type,
                    AccountIcon: account.Icon.Name,
                    AccountIconColor: account.Icon.Color)),
                AccountName: transaction.TransactionSummary!.Account.Name,
                AccountType: transaction.TransactionSummary!.Account.Type.DisplayName(),
                AccountTypeId: transaction.TransactionSummary!.Account.Type);
    }
}
