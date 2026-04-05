using System.Collections.Immutable;
using Result = Kaesseli.Contracts.Features.Accounts.GetAccountingPeriodsContract.Result;

namespace Kaesseli.Features.Accounts;

public static class GetAccountingPeriods
{
    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(CancellationToken cancellationToken);
    }

    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(CancellationToken cancellationToken)
        {
            var result = await repo.GetAccountingPeriods(cancellationToken);
            return result.Select(ap => new Result(
                Id: ap.Id,
                Description: ap.Description,
                FromInclusive: ap.FromInclusive,
                ToInclusive: ap.ToInclusive)).ToImmutableList();
        }
    }
}
