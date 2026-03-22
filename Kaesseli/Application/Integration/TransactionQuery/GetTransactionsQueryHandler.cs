using System.Collections.Immutable;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.TransactionQuery;

public interface IGetTransactionsQueryHandler
{
    Task<IEnumerable<GetTransactionsQueryResult>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class GetTransactionsQueryHandler : IGetTransactionsQueryHandler
{
    private readonly ITransactionRepository _repo;

    public GetTransactionsQueryHandler(ITransactionRepository repo) =>
        _repo = repo;

    public async Task<IEnumerable<GetTransactionsQueryResult>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repo.GetTransactions(request.TransactionSummaryId, cancellationToken);
        return transactions.Select(transaction => transaction.ToGetTransactionSummary()).ToImmutableList();
    }
}
