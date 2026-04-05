using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;

namespace Kaesseli.Features.Integration.NextOpenTransaction;

public static class GetNextOpenTransaction
{
    public record Query(int Skip);

    public record Result(
        Guid Id,
        decimal Amount,
        string Description,
        DateOnly ValueDate,
        string AccountName,
        string AccountType,
        AccountType AccountTypeId,
        IEnumerable<SuggestedAccount> SuggestedAccounts);

    public record SuggestedAccount(
        double Relevance,
        Guid AccountId,
        string AccountName,
        string AccountType,
        AccountType AccountTypeId,
        string AccountIcon,
        string AccountIconColor);

    public interface IHandler
    {
        Task<Result?> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository transactionRepository, IAccountRepository accountRepository) : IHandler
    {
        public async Task<Result?> Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await transactionRepository.GetNextOpenTransaction(request.Skip, cancellationToken);
            if (transaction is null) return null;

            var accounts = await accountRepository.GetAccounts(cancellationToken);

            return new Result(
                Id: transaction.Id,
                Amount: transaction.Amount,
                ValueDate: transaction.ValueDate,
                Description: transaction.Description,
                SuggestedAccounts: accounts.Select(account => new SuggestedAccount(
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
}
