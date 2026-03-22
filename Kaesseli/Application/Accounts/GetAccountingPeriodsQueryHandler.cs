using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Accounts;

public class GetAccountingPeriodsQueryHandler: IRequestHandler<GetAccountingPeriodsQuery, IEnumerable<GetAccountingPeriodsQueryResult>>
{
    private readonly IAccountRepository _repo;

    public GetAccountingPeriodsQueryHandler(IAccountRepository repo) =>
        _repo = repo;

    public async Task<IEnumerable<GetAccountingPeriodsQueryResult>> Handle(GetAccountingPeriodsQuery request, CancellationToken cancellationToken)
    {
        var result= await _repo.GetAccountingPeriods(cancellationToken);
        return result.Select(ap => ap.ToGetAccountingPeriodsQueryResult()).ToImmutableList();
    }
}