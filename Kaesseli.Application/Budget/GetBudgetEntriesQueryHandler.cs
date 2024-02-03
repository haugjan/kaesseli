using Kaesseli.Domain.Budget;
using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQueryHandler :
    IRequestHandler<GetBudgetEntriesQuery, IEnumerable<GetBudgetEntriesQueryResult>>
{
    private readonly IBudgetRepository _repository;

    public GetBudgetEntriesQueryHandler(IBudgetRepository repository) =>
        _repository = repository;

    public async Task<IEnumerable<GetBudgetEntriesQueryResult>> Handle(
        GetBudgetEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await _repository.GetBudgetEntries(
                          request: new GetBudgetEntriesRequest
                          {
                              AccountId = request.AccountId, FromDate = request.FromDate, ToDate = request.ToDate
                          },
                          cancellationToken);
        return entries.ToList()
                      .Select(
                          entry => new GetBudgetEntriesQueryResult
                          {
                              Id = entry.Id,
                              Amount = entry.Amount,
                              Description = entry.Description,
                              AccountId = entry.Account.Id,
                              ValueDate = entry.ValueDate
                          });
    }
}