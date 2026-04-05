using System.Collections.Immutable;
using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Journal;

public static class GetJournalEntries
{
    public record Query(Guid AccountingPeriodId, Guid? AccountId, AccountType? AccountType);

    public record Result(Guid Id, Guid? DebitAccountId, Guid? CreditAccountId, decimal Amount, string Description, DateOnly ValueDate);

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IJournalRepository repository) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entries = await repository.GetJournalEntries(
                              request.AccountingPeriodId, accountId: null, request.AccountType,
                              cancellationToken);
            return entries.Select(
                              entry => new Result(
                                  Id: entry.Id,
                                  Amount: entry.Amount,
                                  Description: entry.Description,
                                  DebitAccountId: entry.DebitAccount.Id,
                                  CreditAccountId: entry.CreditAccount.Id,
                                  ValueDate: entry.ValueDate)).ToImmutableList();
        }
    }
}
