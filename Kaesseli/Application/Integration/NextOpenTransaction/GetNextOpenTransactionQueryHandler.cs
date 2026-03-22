using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public interface IGetNextOpenTransactionQueryHandler
{
    Task<GetNextOpenTransactionQueryResult?> Handle(GetNextOpenTransactionQuery request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class GetNextOpenTransactionQueryHandler : IGetNextOpenTransactionQueryHandler
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
