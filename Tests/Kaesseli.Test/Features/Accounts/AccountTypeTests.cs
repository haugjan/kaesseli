using Kaesseli.Features.Accounts;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AccountTypeTests
{
    [Theory]
    [InlineData(AccountType.Asset, 1)]
    [InlineData(AccountType.Liability, 2)]
    [InlineData(AccountType.Revenue, 3)]
    [InlineData(AccountType.Expense, 4)]
    public void AccountType_ShouldMatchExpectedValue(AccountType accountType, int expectedValue)
    {
        // Act
        var accountTypeValue = (int)accountType;

        // Assert
        accountTypeValue.ShouldBe(expectedValue);
    }
}
