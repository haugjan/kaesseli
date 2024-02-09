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
                          request: new GetJournalEntriesRequest
                          {
                              DebitAccountId = request.DebitAccountId,
                              CreditAccountId = request.CreditAccountId,
                              FromDate = request.FromDate, 
                              ToDate = request.ToDate
                          },
                          cancellationToken);
        return entries.ToList()
                      .Select(
                          entry => new GetJournalEntriesQueryResult
                          {
                              Id = entry.Id,
                              Amount = entry.Amount,
                              Description = entry.Description,
                              DebitAccountId = entry.DebitAccount.Id,
                              CreditAccountId = entry.CreditAccount.Id,
                              ValueDate = entry.ValueDate
                          });
    }
}