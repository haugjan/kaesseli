using MediatR;

namespace Kaesseli.Application.Accounts;

public class GetAccountsSummaryQuery : IRequest<IEnumerable<GetAccountsSummaryQueryResult>>
{
    public required Guid AccountingPeriodId { get; init; }
};