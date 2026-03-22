namespace Kaesseli.Application.Accounts;

public class GetAccountsSummaryQuery
{
    public required Guid AccountingPeriodId { get; init; }
}