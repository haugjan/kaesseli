using System.Collections.Immutable;
using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration.TransactionQuery;

public interface IGetTransactionSummariesQueryHandler
{
    Task<IEnumerable<GetTransactionSummariesQueryResult>> Handle(GetTransactionSummariesQuery request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class GetTransactionSummariesQueryHandler : IGetTransactionSummariesQueryHandler
{
    private readonly ITransactionRepository _repository;

    public GetTransactionSummariesQueryHandler(ITransactionRepository repository) =>
        _repository = repository;

    public async Task<IEnumerable<GetTransactionSummariesQueryResult>> Handle(
        GetTransactionSummariesQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await _repository.GetTransactionSummaries(cancellationToken);
        return entries.Select(entry => entry.ToGetTransactionSummary())
                      .ToImmutableList();
    }
}
