using System.Collections.Immutable;

namespace Kaesseli.Features.Accounts;

public static class GetAccountingPeriods
{
    public record Query;

    public record Result(Guid Id, string Description, DateOnly FromInclusive, DateOnly ToInclusive);

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await repo.GetAccountingPeriods(cancellationToken);
            return result.Select(ap => ap.ToGetAccountingPeriodsQueryResult()).ToImmutableList();
        }
    }
}
