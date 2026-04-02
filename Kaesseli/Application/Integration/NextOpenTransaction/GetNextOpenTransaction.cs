using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

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

            return transaction.ToGetNextOpenTransactionResult(accounts);
        }
    }
}
