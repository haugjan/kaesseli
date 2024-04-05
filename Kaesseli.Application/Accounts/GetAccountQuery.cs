using MediatR;

namespace Kaesseli.Application.Accounts;

public class GetAccountQuery : IRequest<GetAccountQueryResult>
{
    public required Guid AccountId { get; init; }
    public required Guid AccountingPeriodId { get; init; }
}