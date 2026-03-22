namespace Kaesseli.Application.Accounts;

public class GetFinancialOverviewCommand
{
    public required Guid AccountingPeriodId { get; init; }
}