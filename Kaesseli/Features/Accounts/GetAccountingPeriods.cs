using System.Collections.Immutable;

namespace Kaesseli.Features.Accounts;

public static class GetAccountingPeriods
{
    public interface IHandler
    {
        Task<IEnumerable<Contracts.Accounts.AccountingPeriod>> Handle(CancellationToken cancellationToken);
    }

    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task<IEnumerable<Contracts.Accounts.AccountingPeriod>> Handle(CancellationToken cancellationToken)
        {
            var result = await repo.GetAccountingPeriods(cancellationToken);
            return result.Select(ap => new Contracts.Accounts.AccountingPeriod(
                Id: ap.Id,
                Description: ap.Description,
                FromInclusive: ap.FromInclusive,
                ToInclusive: ap.ToInclusive)).ToImmutableList();
        }
    }
}
