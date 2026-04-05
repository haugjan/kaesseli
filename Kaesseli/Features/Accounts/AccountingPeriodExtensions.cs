
namespace Kaesseli.Features.Accounts;

public static class AccountingPeriodExtensions
{
    public static GetAccountingPeriods.Result ToGetAccountingPeriodsQueryResult(this AccountingPeriod accountingPeriod) =>
        new(
            Id: accountingPeriod.Id,
            Description: accountingPeriod.Description,
            FromInclusive: accountingPeriod.FromInclusive,
            ToInclusive: accountingPeriod.ToInclusive);
}
