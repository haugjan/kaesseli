using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public interface IGetAccountingPeriodsQueryHandler
{
    Task<IEnumerable<GetAccountingPeriodsQueryResult>> Handle(GetAccountingPeriodsQuery request, CancellationToken cancellationToken);
}

public class GetAccountingPeriodsQueryHandler : IGetAccountingPeriodsQueryHandler
{
    private readonly IAccountRepository _repo;

    public GetAccountingPeriodsQueryHandler(IAccountRepository repo) =>
        _repo = repo;

    public async Task<IEnumerable<GetAccountingPeriodsQueryResult>> Handle(GetAccountingPeriodsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repo.GetAccountingPeriods(cancellationToken);
        return result.Select(ap => ap.ToGetAccountingPeriodsQueryResult()).ToImmutableList();
    }
}
