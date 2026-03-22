namespace Kaesseli.Application.Accounts;

public class GetAccountQuery
{
    public required Guid AccountId { get; init; }
    public required Guid AccountingPeriodId { get; init; }
}