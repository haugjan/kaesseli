using System.Collections.Immutable;
using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Integration;

// ReSharper disable once UnusedType.Global
public class GetTransactionSummariesQueryHandler :
    IRequestHandler<GetTransactionSummariesQuery, IEnumerable<GetTransactionSummariesQueryResult>>
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