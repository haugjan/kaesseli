using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public static class GetAccountingPeriods
{
    public record Query;

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required string Description { get; init; }
        public required DateOnly FromInclusive { get; init; }
        public required DateOnly ToInclusive { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        private readonly IAccountRepository _repo;

        public Handler(IAccountRepository repo) =>
            _repo = repo;

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetAccountingPeriods(cancellationToken);
            return result.Select(ap => ap.ToGetAccountingPeriodsQueryResult()).ToImmutableList();
        }
    }
}
