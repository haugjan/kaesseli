using Kaesseli.Features.Accounts;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AccountingPeriodValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyDescription_Throws(string? description)
    {
        Should.Throw<ArgumentException>(() =>
            AccountingPeriod.Create(description!, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
    }
}
