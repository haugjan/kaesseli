using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public static class AccountingPeriodExtensions
{
    public static GetAccountingPeriodsQueryResult ToGetAccountingPeriodsQueryResult(this AccountingPeriod accountingPeriod) =>
        new()
        {
            Id = accountingPeriod.Id,
            Description = accountingPeriod.Description,
            FromInclusive = accountingPeriod.FromInclusive,
            ToInclusive = accountingPeriod.ToInclusive
        };
}