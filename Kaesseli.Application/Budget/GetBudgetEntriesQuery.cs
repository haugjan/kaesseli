using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Budget;

public class GetBudgetEntriesQuery : IRequest<IEnumerable<GetBudgetEntriesQueryResult>>
{
    public  Guid? AccountId { get; init; }
    public  DateOnly? FromDate { get; init; }
    public  DateOnly? ToDate { get; init; }
    public  AccountType? AccountType { get; init; }
}