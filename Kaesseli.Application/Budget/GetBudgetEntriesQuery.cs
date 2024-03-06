using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQuery : IRequest<IEnumerable<GetBudgetEntriesQueryResult>>
{
    public required Guid? AccountId { get; init; }
    public required AccountType? AccountType { get; init; }
    public required Guid? AccountingPeriodId { get; init; }
}