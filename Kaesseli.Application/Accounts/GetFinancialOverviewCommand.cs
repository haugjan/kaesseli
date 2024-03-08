using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Accounts;

public class GetFinancialOverviewCommand : IRequest<GetFinancialOverviewCommandResult>
{
    public required Guid AccountingPeriodId { get; init; }
}