using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public static class GetNextOpenTransaction
{
    public record Query
    {
        public required int Skip { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required decimal Amount { get; init; }
        public required string Description { get; init; }
        public required DateOnly ValueDate { get; init; }
        public required string AccountName { get; init; }
        public required string AccountType { get; init; }
        public required AccountType AccountTypeId { get; init; }
        public required IEnumerable<SuggestedAccount> SuggestedAccounts { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public class SuggestedAccount
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required double Relevance { get; init; }
        public required Guid AccountId { get; init; }
        public required string AccountName { get; init; }
        public required string AccountType { get; init; }
        public required AccountType AccountTypeId { get; init; }
        public required string AccountIcon { get; init; }
        public required string AccountIconColor { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<Result?> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler : IHandler
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;

        public Handler(ITransactionRepository transactionRepository, IAccountRepository accountRepository)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result?> Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetNextOpenTransaction(request.Skip, cancellationToken);
            if (transaction is null) return null;

            var accounts = await _accountRepository.GetAccounts(cancellationToken);

            return transaction.ToGetNextOpenTransactionResult(accounts);
        }
    }
}
