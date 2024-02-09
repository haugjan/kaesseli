using FluentAssertions;
using Kaesseli.Domain.Common;
using Xunit;

namespace Kaesseli.Domain.Test.Common;

public class AccountTypeTheoryTests
{
    [Theory]
    [InlineData(AccountType.Asset, 1)]
    [InlineData(AccountType.Liability, 2)]
    [InlineData(AccountType.Revenue, 3)]
    [InlineData(AccountType.Expense, 4)]
    public void AccountType_ShouldMatchExpectedValue(AccountType accountType, int expectedValue) =>
        // Act & Assert
        ((int)accountType).Should().Be(expectedValue);
}