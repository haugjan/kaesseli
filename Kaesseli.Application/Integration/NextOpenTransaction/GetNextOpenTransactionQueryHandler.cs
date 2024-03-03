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

        return transaction.ToGetNextOpenTransactionResult(accounts);
    }
}