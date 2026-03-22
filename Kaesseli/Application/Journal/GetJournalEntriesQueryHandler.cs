using System.Collections.Immutable;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Journal;

// ReSharper disable once UnusedType.Global
public class GetJournalEntriesQueryHandler(IJournalRepository repository) :
    IRequestHandler<GetJournalEntriesQuery, IEnumerable<GetJournalEntriesQueryResult>>
{
    public async Task<IEnumerable<GetJournalEntriesQueryResult>> Handle(
        GetJournalEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await repository.GetJournalEntries(
                          request.AccountingPeriodId, accountId: null, request.AccountType,
                          cancellationToken);
        return entries.Select(
                          entry => new GetJournalEntriesQueryResult
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