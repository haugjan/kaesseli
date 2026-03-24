using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Xunit;

namespace Kaesseli.Test.Domain.Common;

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
        accountTypeValue.Should().Be(expectedValue);
    }
}
