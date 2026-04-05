using Kaesseli.Features.Accounts;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Domain.Accounts;

public class AccountingPeriodTests
{
    [Fact]
    public void Create_FromAfterTo_Throws()
    {
        var from = new DateOnly(2025, 12, 31);
        var to = new DateOnly(2025, 1, 1);

        Should.Throw<ArgumentOutOfRangeException>(() =>
            AccountingPeriod.Create("2025", from, to));
    }

    [Fact]
    public void Create_FromEqualsTo_Succeeds()
    {
        var date = new DateOnly(2025, 1, 1);

        var period = AccountingPeriod.Create("2025", date, date);

        period.FromInclusive.ShouldBe(date);
        period.ToInclusive.ShouldBe(date);
    }
}
