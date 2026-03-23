using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Journal;

public static class GetJournalEntries
{
    public record Query
    {
        public required Guid AccountingPeriodId { get; init; }
        public required Guid? AccountId { get; init; }
        public required AccountType? AccountType { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required Guid? DebitAccountId { get; init; }
        public required Guid? CreditAccountId { get; init; }
        public required decimal Amount { get; init; }
        public required string Description { get; init; }
        public required DateOnly ValueDate { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

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
                              entry => new Result
                              {
                                  Id = entry.Id,
                                  Amount = entry.Amount,
                                  Description = entry.Description,
                                  DebitAccountId = entry.DebitAccount.Id,
                                  CreditAccountId = entry.CreditAccount.Id,
                                  ValueDate = entry.ValueDate
                              }).ToImmutableList();
        }
    }
}
