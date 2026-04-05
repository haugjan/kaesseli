
namespace Kaesseli.Features.Accounts;

public static class AccountingPeriodExtensions
{
    extension(AccountingPeriod accountingPeriod)
    {
        public GetAccountingPeriods.Result ToGetAccountingPeriodsQueryResult() =>
            new(
                Id: accountingPeriod.Id,
                Description: accountingPeriod.Description,
                FromInclusive: accountingPeriod.FromInclusive,
                ToInclusive: accountingPeriod.ToInclusive);
    }
}
