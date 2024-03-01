using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once UnusedType.Global
public class GetNextOpenTransactionQueryHandler : IRequestHandler<GetNextOpenTransactionQuery, GetNextOpenTransactionQueryResult?>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;

    public GetNextOpenTransactionQueryHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    public async Task<GetNextOpenTransactionQueryResult?> Handle(
        GetNextOpenTransactionQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetNextOpenTransaction(request.Skip, cancellationToken);
        if (transaction is null) return null;
        
        var accounts = await _accountRepository.GetAccounts(cancellationToken);

        return new()
        {
            TransactionId = transaction.Id,
            Amount = transaction.Amount,
            ValueDate = transaction.ValueDate,
            Description = transaction.Description,
            SuggestedAccounts = accounts.Select(
                account => new SuggestedAccount
                {
                    Relevance = 1,
                    AccountName = account.Name,
                    AccountType = account.Type.DisplayName(),
                    AccountTypeId = account.Type,
                    AccountIcon = account.Icon,
                    AccountIconColor = account.IconColor
                }),
            AccountName = transaction.TransactionSummary!.Account.Name,
            AccountType = transaction.TransactionSummary!.Account.Type.DisplayName(),
            AccountTypeId = transaction.TransactionSummary!.Account.Type
        };
    }
}