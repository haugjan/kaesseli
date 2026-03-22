using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Xunit;

namespace Kaesseli.Domain.Test.Common;

public class AccountTypeExtensionsTests
{
    [Theory]
    // ReSharper disable StringLiteralTypo
    [InlineData(AccountType.Asset, "Aktiv")]
    [InlineData(AccountType.Liability, "Passiv")]
    [InlineData(AccountType.Expense, "Ausgaben")]
    // ReSharper restore StringLiteralTypo
    public void AccountTypeToString_ShouldReturnExpectedString(AccountType accountType, string expectedString)
    {
        // Act
        var result = accountType.DisplayName();

        // Assert
        result.Should().Be(expectedString);
    }

    [Fact]
    public void AccountTypeToString_ShouldThrowArgumentOutOfRangeExceptionForUndefinedValue()
    {
        // Arrange
        const AccountType accountType = (AccountType)(-1);
        var toDisplayName = () => accountType.DisplayName();

        // Act & Assert
        toDisplayName.Should().Throw<ArgumentOutOfRangeException>();
    }
}
